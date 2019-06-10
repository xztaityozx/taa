using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;

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

            using (var source = new BlockingCollection<FilterDefinition<Record>>()) {
                foreach (var f in request.FindFilterDefinitions(id)) {
                    source.TryAdd(f);
                }

                using (var mpb = new MultiProgressBar("PipeLine"))
                using (var secondQueue = new BlockingCollection<Record[]>())
                using (var thirdQueue = new BlockingCollection<Document>())
                using (var fourthQueue = new BlockingCollection<Tuple<string, long>[]>()) {
                    var first = new PipeLineFilter<FilterDefinition<Record>, Record[]>(
                        f => repo.Find(f).ToArray(),
                        source,
                        secondQueue,
                        Parallel / 4,
                        "Pulling records from database",
                        Logger,
                        cts.Token
                    );
                    var second = new PipeLineFilter<Record[], Document>(
                        r => new Document(r, r.First().Seed, Sweeps),
                        secondQueue,
                        thirdQueue,
                        Parallel / 4,
                        "Building documents",
                        Logger,
                        cts.Token
                    );
                    var third = new PipeLineFilter<Document, Tuple<string, long>[]>(
                        d => filter.Aggregate(d, mpb).Zip(filter.ExpressionStringList, (l, s) => Tuple.Create(s, l))
                            .ToArray(),
                        thirdQueue, fourthQueue,
                        Parallel / 4, "Aggregating",
                        Logger,
                        cts.Token
                    );

                }
            }


            return true;
        }
    }
}