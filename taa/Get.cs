using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;

namespace taa {
    [Verb("get", HelpText = "数え上げます")]
    public class Get : SubCommand {
        public IEnumerable<string> Request { get; set; }

        [Option("start", Default = 1, HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }

        [Option("end", Default = 2000, HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }


        public override bool Run() {
            LoadConfig();
            var filter = new Filter(Config);

            // 評価に使うデリゲートを作る
            Spinner.Start("Building Filters...", spin => {
                try {
                    foreach (var s in filter.Build()) {
                        spin.Text = s;
                    }

                    spin.Info("Finished");
                }
                catch (Exception e) {
                    spin.Fail($"Failed: {e}");
                    throw;
                }
            });
            var request = new Request {
                Keys = filter.KeyList.ToList(),
                Sweeps = Sweeps,
                Vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                Vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation),
                SeedEnd = SeedEnd,
                SeedStart = SeedStart
            };

            var d = new Dispatcher(Config);
            var cts = new CancellationTokenSource();
            var res = d.Dispatch(cts.Token, request, filter);

            foreach (var item in res) {
                item.WL();
            }

            return true;
        }
    }
}