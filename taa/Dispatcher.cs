using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace taa {
    public class Request {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public IReadOnlyCollection<Tuple<string, decimal>> TargetList { get; private set; }
        public int SeedStart { get; set; }
        public int SeedEnd { get; set; }
        public int Sweep { get; set; }

        public int Size => TargetList.Count * (SeedEnd - SeedStart + 1);

        public Request(
            double vtnVoltage, double vtnSigma, double vtnDeviation,
            double vtpVoltage, double vtpSigma, double vtpDeviation,
            int seedStart, int seedEnd,
            int sweep,
            IEnumerable<Tuple<string, decimal>> targets
        ) {
            Vtn=new Transistor(vtnVoltage, vtnSigma, vtnDeviation);
            Vtp=new Transistor(vtpVoltage, vtpSigma, vtpDeviation);
            SeedStart = seedStart;
            SeedEnd = seedEnd;
            Sweep = sweep;
            TargetList = targets.ToList();
        }
    }
    
    public class Dispatcher {
        private readonly Config config;

        public Dispatcher(string configFile) {
            string str;
            using (var sr = new StreamReader(configFile)) {
                str = sr.ReadToEnd();
            }

            var d = new Deserializer();
            config = d.Deserialize<Config>(str);
        }

        public Dispatcher(Config config) => this.config = config;

        private readonly ProgressBarOptions option = new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Blue,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '█',
            BackgroundCharacter = '▒'

        };

        private readonly ProgressBarOptions childOption = new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '>',
        };

        public Tuple<string, long>[] Start(CancellationToken token, Request request) {
            const int mainProcesses = 3;
            var expressionCount = config.ExpressionList.Count;

            var result = new long[expressionCount];
            
         

            return result.Zip(config.ExpressionList, (l, s) => Tuple.Create(s, l)).ToArray();
        }



        public Tuple<string,long>[] Start(CancellationToken token, string dir) {
            const int mainProcessCount = 3;
            var expressionCount = config.ExpressionList.Count;

            var result = new long[expressionCount];

            using (var pb = new ProgressBar(mainProcessCount, "Dispatcher", option)) {
                // Generate Delegates
                List<Func<Map<string, decimal>, bool>> delegates;
                {
                    using (var cpb = pb.Spawn(expressionCount, "Generate Delegates from Expressions", childOption)) {
                        var f = new Filter(config);
                        delegates = f.Build(cpb);
                    }

                    pb.Tick("Finished: Generate Delegates");
                }

                var tasks = new List<Task>();
                using (var mpb = new MultiProgressBar(pb, "Aggregate")) 
                using (var q = new BlockingCollection<Document>()) {
                    var dispatcher = Task.Run(() => {
                        using (var cpb = pb.Spawn(config.Sweep, "Generate Record", childOption)) {
                            Enumerable.Range(1,config.Sweep).AsParallel()
                                .WithDegreeOfParallelism(config.Parallel)
                                .WithCancellation(token)
                                .ForAll(seed => {
                                    // Record を作成
                                    var path = Path.Combine(dir, $"SEED{seed:D5}.csv");
                                    q.TryAdd(new Document(path, config.Sweep, seed));
                                    cpb.Tick();
                                });
                            q.CompleteAdding();
                        }

                    }, token);
                    tasks.Add(dispatcher);

                    for (var i = 0; i < expressionCount; i++) {
                        mpb.AddBar(config.Sweep, config.ExpressionList[i]);
                    }

                    var worker = Task.Run(() => {
                        foreach (var record in q.GetConsumingEnumerable()) {
                            token.ThrowIfCancellationRequested();
                            for (var i = 0; i < expressionCount; i++) {
                                result[i] += record.Count(delegates[i]);
                                mpb.Tick(i);
                            }
                        }

                        pb.Tick();
                    }, token);

                    for (var i = 0; i < config.Parallel; i++) {
                        tasks.Add(worker);
                    }

                    pb.Tick("Finished: All Process");

                    Task.WaitAll(tasks.ToArray());
                }
            }

            return result.Zip(config.ExpressionList, (l, s) => Tuple.Create(s, l)).ToArray();
        }

        private class MultiProgressBar : IDisposable {
            private readonly ChildProgressBar parent;
            private readonly List<ChildProgressBar> list;

            private readonly ProgressBarOptions option = new ProgressBarOptions {
                ForegroundColor = ConsoleColor.DarkBlue,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '-',
                BackgroundCharacter = ' '
            };

            public MultiProgressBar(ProgressBarBase master, string name) {
                parent = master.Spawn(0, name, new ProgressBarOptions {
                    ForegroundColor = ConsoleColor.DarkGreen,
                    BackgroundColor = ConsoleColor.DarkGray,
                    ProgressCharacter = '>',
                    BackgroundCharacter = '>'
                });
                list = new List<ChildProgressBar>();
            }

            public void AddBar(int maxTicks, string name) {
                parent.MaxTicks += maxTicks;
                list.Add(parent.Spawn(maxTicks, name, option));
            }

            public void Tick(int index) {
                list[index].Tick();
                parent.Tick();
            }

            public void Dispose() {
                foreach (var childProgressBar in list) {
                    childProgressBar.Dispose();
                }
                parent.Dispose();
            }
        }
    }
}