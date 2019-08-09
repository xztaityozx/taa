using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using ShellProgressBar;
using taa.Factory;
using taa.Parameter;
using taa.Repository;

namespace taa.Verb {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }

        private Exception Do(IProgressBar bar=null) {
            foreach (var inputFile in InputFiles) {
                try {
                    // ファイル名からSeedを得る
                    var seed = int.Parse(inputFile
                        .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Last());
                    
                    var db = Transistor.ToTableName(
                        new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                        new Transistor(VtpVoltage, VtpSigma, VtpDeviation)
                    );

                    var repo = new MssqlRepository();
                    repo.Use(db);
                    repo.BulkUpsert(RecordFactory.BuildFromCsv(inputFile, seed));

                    if (bar != null) bar.Tick();
                    else Logger.Info($"{inputFile} pushed");
                }
                catch (FormatException e) {
                    base.Logger.Error($"Invalid Filename: {inputFile}");
                    return e;
                }
                catch (Exception e) {
                    base.Logger.Error($"failed push to database. internal exception: {e}");
                    return e;
                }
            }

            return null;
        }

        public override Exception Run() {
            LoadConfig("~/.config/taa/config.yml");

            if (NoProgressBar) {
                return Do();
            }
            using (var bar = new ProgressBar(InputFiles.Count(), "Pushing...", new ProgressBarOptions {
                ForegroundColor = ConsoleColor.DarkBlue,
                BackgroundCharacter = '-',
                ForegroundColorDone = ConsoleColor.Green,
                ProgressCharacter = '>',
                BackgroundColor = ConsoleColor.DarkGray
            })) {
                return Do(bar);
            }
        }
    }
}