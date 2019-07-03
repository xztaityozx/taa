﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Kurukuru;
using Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    [Verb("get", HelpText = "数え上げます")]
    public class Get : SubCommand {

        [Option("start", Default = 1, HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }

        [Option("end", Default = 2000, HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }

        [Option('x',"pullParallel", Default = 1, HelpText = "データベースからデータを取り出すパイプラインの並列数です")]
        public int PullParallel { get; set; }

        [Option('y', "buildParallel", Default = 1, HelpText = "取り出したデータをまとめるパイプラインの並列数です")]
        public int BuildParallel { get; set; }

        [Option('z', "aggregateParallel", Default = 1, HelpText="数え上げの並列数です")]
        public int AggregateParallel { get; set; }

        public override bool Run() {
            LoadConfig();
            var cts = new CancellationTokenSource();
            using (cts) {
                Console.CancelKeyPress += (sender, args) => { cts.Cancel(); };
                Logger.Warn("Press 'Ctrl+C' to cancel");

                var filter = new Filter(Config);

                Logger.Info("Start building filters");
                // 評価に使うデリゲートを作る
                Spinner.Start("Building Filters...", spin => {
                    try {
                        foreach (var s in filter.Build()) {
                            spin.Text = s;
                        }

                        spin.Info("Finished building filters");
                    }
                    catch (Exception e) {
                        spin.Fail($"Failed: {e}");
                        throw;
                    }
                });

                Logger.Info($"{filter.ExpressionStringList} filters was built");

                // Id取得に使うrequest
                var request = new Request {
                    Keys = filter.KeyList.ToList(),
                    Sweeps = Sweeps,
                    Vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                    Vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation),
                    SeedEnd = SeedEnd,
                    SeedStart = SeedStart
                };

                Logger.Info("Start find parameter id");

                var repo = new Repository(Config.Database);
                var id = new ObjectId();
                Spinner.Start("Find parameter id", () => { id = repo.FindParameter(request).Id; });

                Logger.Info($"Parameter Id: {id}");

                const int steps = 3;

                Logger.Info(request);

                var source = Enumerable.Range(SeedStart, SeedEnd - SeedStart + 1).ToArray();
                var size = source.Length;

                PipeLine.PipeLineState status;
                var result = new Map<string, long>();
                using (var pipeline = new PipeLine(cts.Token, steps * size)) {
                    var first = pipeline.Add("Pulling records from database", PullParallel, source,
                        seed => Tuple.Create(repo.Find(request.Keys, seed, id), seed));

                    Logger.Info("Added first stage: pull records");

                    var second = pipeline.Add("Building documents", BuildParallel, first, size,
                        list => new Document(list.Item1, list.Item2, Sweeps));

                    Logger.Info("Added second stage: record => document");

                    var third = pipeline.Add("Aggregating", AggregateParallel, second, size,
                        document => filter.Aggregate(document)
                            .Zip(filter.ExpressionStringList, Tuple.Create));

                    Logger.Info("Added third stage: document => failures");

                    Logger.Info("Start pipeline task");
                    var sw = new Stopwatch();
                    sw.Start();
                    status = pipeline.Invoke(() => {
                        foreach (var res in third.GetConsumingEnumerable()) {
                            foreach (var (value, key) in res) {
                                result[key] += value;
                            }
                        }
                    });
                    sw.Stop();
                    Logger.Info($"Elapsed time: {sw.Elapsed}");
                }

                switch (status) {
                    case PipeLine.PipeLineState.Completed:
                        Logger.Info(status);

                        foreach (var (k, v) in Config.Conditions) {
                            Logger.Info($"{k}: {v}");
                        }

                        Logger.Success("Taa results");
                        foreach (var (k, v) in result) {
                            $"{k}: {v}".WL();
                        }

                        return true;
                    case PipeLine.PipeLineState.Canceled:
                        Logger.Warn(status);
                        return false;
                    case PipeLine.PipeLineState.Failed:
                        Logger.Error(status);
                        return false;
                    case PipeLine.PipeLineState.Unknown:
                        Logger.Error(status);
                        return false;
                    default:
                        Logger.Throw("", new ArgumentOutOfRangeException());
                        return false;
                }
            }
        }
    }
}