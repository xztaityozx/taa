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

        private readonly ProgressBarOptions innerOption = new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Magenta,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '-',
            BackgroundCharacter = ' '
        };


        public long[] Start(CancellationToken token, string dir) {
            var mainProcessCount = 3;
            var expressionCount = config.ExpressionList.Count;
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

                var tasks = new List<Task<long>>();
                using (var q = new BlockingCollection<Record>()) {
                    tasks.Add(Dispatch(token, dir, q, pb));

                    using (var cpb = pb.Spawn(expressionCount, "Aggregate Master", childOption)) {
                        for (var i = 0; i < expressionCount; i++) {
                            tasks.Add(Worker(token, q, delegates[i], config.ExpressionList[i], cpb));
                        }
                    }

                    pb.Tick("Finished: All Process");

                    return Task.WhenAll(tasks).Result.Skip(1).ToArray();
                }
            }
        }


        private Task<long> Dispatch(CancellationToken token, string dir, BlockingCollection<Record> queue,
            ProgressBarBase pb) {
            using (var cpb = pb.Spawn(config.Range, "Generate Records", innerOption)) {
                Enumerable.Range(1, config.Range).AsParallel()
                    .WithDegreeOfParallelism(config.Parallel)
                    .WithCancellation(token)
                    .ForAll(seed => {
                        queue.TryAdd(new Record(dir, config.Signals, config.Times, seed), Timeout.Infinite);

                        cpb.Tick();
                    });
            }

            queue.CompleteAdding();
            pb.Tick("Finished: Generate Record");
            return Task.FromResult(0L);
        }

        private Task<long> Worker(CancellationToken token, BlockingCollection<Record> queue,
            Func<Map<string, decimal>, bool> filter, string name, ProgressBarBase parent) {
            var result = new long[config.Range];
            using (var pb = parent.Spawn(config.Range, $"{name}", childOption)) {
                foreach (var record in queue.GetConsumingEnumerable()) {
                    token.ThrowIfCancellationRequested();
                    result[record.Seed - 1] = record.Count(filter);
                    pb.Tick();
                }
            }

            parent.Tick($"Finished: {name}");

            return Task.FromResult(result.Sum());
        }
    }
}