using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using ShellProgressBar;
using taa.Factory;
using taa.Model;
using taa.Parameter;
using taa.Repository;

namespace taa.Verb {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('p', "Parallel", HelpText = "並列数です", Default = 1)]
        public int Parallel { get; set; }


        private const int QueueBuffer = 50000;

        public override Exception Run() {
            LoadConfig("~/.config/taa/config.yml");

            Logger.Info("Start push");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) => cts.Cancel();

            try {
                var repo = new MssqlRepository();
                var name = Transistor.ToTableName(
                    new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                    new Transistor(VtpVoltage, VtpSigma, VtpDeviation)
                );
                repo.Use(name);

                using (var pipeline = new PipeLine.PipeLine(cts.Token)) {
                    var first = pipeline.AddSelectMany(Parallel, QueueBuffer, InputFiles, RecordFactory.BuildFromCsv);
                    first.OnInterval += s => Logger.Info(s);
                    pipeline.Invoke(() => {
                        var list = new List<RecordModel>();

                        foreach (var r in first.Results.GetConsumingEnumerable()) {
                            list.Add(r);
                            if (list.Count != QueueBuffer) continue;
                            repo.BulkUpsert(list);
                            list = new List<RecordModel>();
                        }
                    
                        if (list.Any()) repo.BulkUpsert(list);
                    });
                    Logger.Info("Finished");
                }
            }
            catch (OperationCanceledException e) {
                return e;
            }

            return null;
        }
    }

}