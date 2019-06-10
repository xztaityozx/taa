using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }


        public override bool Run() {
            LoadConfig();
            Logger.Info("Start Push");
            var repo = new Repository(Config.Database);

            var p = new Parameter {
                Vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                Vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation),
                Sweeps = Sweeps
            };

            var size = InputFiles.Count();
            foreach (var item in InputFiles.Select((s, i) => new{s,i})) {
                var e = PushTask(p, item.s, repo, item.i+1, size);
                if (e != null) {
                    Logger.Throw("Failed push", e);
                }
            }

            Logger.Info("Finished Push");
            return true;
        }

        private static Exception PushTask(Parameter p, string file, Repository repo, int cnt, int size) {
            try {
                var seed = int.Parse(file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                    .Last().Substring("SEED".Length, 5));

                Spinner.Start("", spin => {
                    foreach (var s in repo.Push(p.Vtn, p.Vtp, p.Sweeps, file, seed)) {
                        spin.Text = s;
                    }

                    spin.Info($"Pushed: {cnt}/{size}");
                });
            }
            catch (Exception e) {
                return e;
            }

            return null;
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }
    }
}