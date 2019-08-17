using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using ShellProgressBar;
using taa.Extension;
using taa.Factory;
using taa.Model;
using taa.Parameter;
using taa.PipeLine;
using taa.Repository;

namespace taa.Verb {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('d',"directory", HelpText = "指定したディレクトリの下にあるファイルをすべてPushします")]
        public string TargetDirectory { get; set; }

        [Option('p', "Parallel", HelpText = "並列数です", Default = 1)]
        public int Parallel { get; set; }

        [Option('b',"bufferSize", Default = 50000, HelpText = "DBへのBulkUpsert一回当たりのEntityの個数です")]
        public int QueueBuffer { get; set; }

        private Exception PushFiles(CancellationToken token, ProgressBarBase parentBar, ProgressBarBase parseBar, ProgressBarBase pushBar) {
            
            try {
                var repo = new MssqlRepository();
                var name = Transistor.ToTableName(
                    new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                    new Transistor(VtpVoltage, VtpSigma, VtpDeviation)
                );
                repo.Use(name);

                using (var pipeline = new PipeLine.PipeLine(token)) {
                    var first = pipeline.AddSelectMany(Parallel, QueueBuffer, InputFiles, RecordFactory.BuildFromCsv);
                    first.OnInterval += s => parseBar?.Tick($"parsed: {s}");

                    first.OnFinish += () => parentBar?.Tick($"Finished parse csv files. {first.TotalResultsCount} records were parsed");

                    pipeline.Invoke(async () => {
                        var list = new List<Record>();
                        var sum = 0;

                        foreach (var r in first.Results.GetConsumingEnumerable()) {
                            list.Add(r);
                            pushBar.MaxTicks++;

                            if (list.Count != QueueBuffer) continue;
                            sum += list.Count;
                            await pushBar.TickWithPush(list, token, repo, "pushed");

                            list = new List<Record>();
                        }

                        if (!list.Any()) return;

                        sum += list.Count;
                        repo.BulkUpsert(list);

                        //pushBar.TickDelta(token, list.Count, $"{sum} records were pushed", () => {
                        //});
                    });

                    parentBar.Tick();
                }
            }
            catch (OperationCanceledException e) {
                return e;
            }
            catch (Exception e) {
                return e;
            }

            return null;
        }

        public override Exception Run(CancellationToken token) {
            LoadConfig("~/.config/taa/config.yml");

            InputFiles = InputFiles.Any() ? InputFiles : Directory.EnumerateFiles(TargetDirectory);
            
            Logger.Info("Start push");
            using (var bar = new ProgressBar(2, "Master", new ProgressBarOptions {
                ForegroundColor = ConsoleColor.DarkGreen,
                BackgroundCharacter = '-',
                ProgressCharacter = '>',
                CollapseWhenFinished = false
            })) {
                var opt = new ProgressBarOptions {
                    ForegroundColor = ConsoleColor.DarkYellow,
                    ProgressCharacter = '=',
                    CollapseWhenFinished = false
                };
                using(var pushBar=bar.Spawn(0,"Pushing...", opt))
                using (var parseBar = bar.Spawn(InputFiles.Count(), "Parsing...", opt))
                    return PushFiles(token, bar, parseBar, pushBar);
            }

        }
    }

}