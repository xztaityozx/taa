using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option("seed", Default = 1, HelpText = "Seedの値です")]
        public int Seed { get; set; }

        public override bool Run() {
            LoadConfig();
            Logger.Info("Start Push");
            var repo = new Repository(Config.Database);

            var vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation);
            var vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation);

            var parameter = new Parameter {
                Vtn=vtn,
                Vtp=vtp,
                Sweeps = Sweeps,
            };
            
            Spinner.Start("Updating...", () => { parameter.Id = repo.FindParameter(vtn, vtp, Sweeps).Id; });

            var d = new Dispatcher(Config);
            var cts = new CancellationTokenSource();

            var e = d.DispatchPushing(cts.Token, parameter, InputFiles.ToArray());
            if (e != null) {
                foreach (var exception in e) {
                    exception.WL();
                }
                return false;
            }

            Logger.Info("Finished Push");
            return true;
        }
        
        

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }
    }
}