using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;
using ShellProgressBar;

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

            var cts = new CancellationTokenSource();

            var repo = new Repository(Config.Database);
            var id = repo.FindParameter(request).Id;

            const int steps = 3;
            var parallelPerStage = Parallel / steps;

            var source = Enumerable.Range(SeedStart, SeedEnd - SeedStart + 1).Select(seed =>
                Tuple.Create(Builders<Record>.Filter.Where(
                    r => r.Seed == seed && request.Keys.Contains(r.Key) && r.ParameterId == id), seed)
            ).ToArray();
            
            using (var pipeline = new PipeLine(cts.Token, steps)) {
                var first = pipeline.Add("Pulling records from database", parallelPerStage, source, source.Length,
                    f => Tuple.Create(repo.Find(f.Item1), f.Item2));

                var second = pipeline.Add("Building documents", parallelPerStage, first, source.Length,
                    list => new Document(list.Item1, list.Item2, Sweeps));

                var third = pipeline.Add("Aggregating", parallelPerStage, second, source.Length,
                    document => filter.Aggregate(document)
                        .Zip(filter.ExpressionStringList, Tuple.Create));
                
            }

            return true;
        }
    }
}