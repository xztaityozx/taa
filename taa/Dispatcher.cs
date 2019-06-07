using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kurukuru;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    using RecordQueue = BlockingCollection<Record[]>;
    using SeedFilePair = Tuple<int, string>;

    public class Dispatcher {
        private readonly Config config;

        public Dispatcher(Config config) => this.config = config;

        public IEnumerable<Tuple<string, long>>
            DispatchPulling(CancellationToken token, Request request, Filter filter) {
            var expressionCount = config.Expressions.Count;
            var rt = new long[expressionCount];

            IEnumerable<Record> records;
            using (var pb = new ProgressBar(request.Size, "Pulling records from Database", ConsoleColor.DarkBlue)) {
                var repo = new Repository(config.Database);
                records = repo.Pull(request, pb).ToArray();
            }

            IEnumerable<Document> documents = null;
            Spinner.Start("Generating documents...", spin => {
                try {
                    documents = Document.BuildDocuments(records);
                    spin.Info("Finished");
                }
                catch (Exception e) {
                    spin.Fail($"Failed: {e}");
                    throw;
                }
            });

            var size = records.Count();

            using (var mpb = new MultiProgressBar("Aggregating...")) {
                foreach (var exp in filter.ExpressionStringList) {
                    mpb.AddProgressBar(exp, records.Count());
                }

                documents.AsParallel()
                    .WithCancellation(token)
                    .WithDegreeOfParallelism(config.Parallel)
                    .ForAll(item => {
                        var res = filter.Aggregate(item, mpb);
                        for (var i = 0; i < expressionCount; i++) {
                            rt[i] += res[i];
                        }
                    });
            }


            return filter.ExpressionStringList.Zip(rt, Tuple.Create);
        }

        public Exception[] DispatchPushing(
            CancellationToken token,
            Parameter parameter,
            string[] files) {
            var length = files.Length;
            var box = MakeSeedFilePair(files);

            var proParallel = config.Parallel / 2;
            var conParallel = config.Parallel - proParallel;

            var repo = new Repository(config.Database);

            var tasks = new List<Task<Exception>>();
            var cts = new CancellationTokenSource();

            using (var pb = new ProgressBar(2, "", ConsoleColor.DarkCyan))
            using (var recordPb = pb.Spawn(length, "Generating..."))
            using (var pushPb = pb.Spawn(length, "Pushing..."))
            using (var q = new RecordQueue()) {
                tasks.Add(Task.Run(() => Producer(cts.Token, q, recordPb, proParallel, parameter, box), token));
                for (var i = 0; i < conParallel; i++)
                    tasks.Add(Task.Run(() => Consumer(q, pushPb, repo, parameter.Id), token));
                var result = Task.WhenAll(tasks).Result;

                return result.Where(item => item != null).ToArray();
            }
        }

        private static Exception Producer(CancellationToken token, RecordQueue queue, IProgressBar bar, int parallel,
            Parameter parameter, IEnumerable<SeedFilePair> files) {
            try {
                files.AsParallel().WithCancellation(token).WithDegreeOfParallelism(parallel)
                    .Select(item => new Document(item.Item2, item.Item1, parameter.Sweeps))
                    .Select(item => item.GenerateRecords(parameter.Vtn, parameter.Vtp).ToArray())
                    .ForAll(item => {
                        queue.TryAdd(item);
                        bar.Tick();
                    });

                queue.CompleteAdding();
                return null;
            }
            catch (Exception e) {
                return e;
            }
        }

        private static Exception Consumer(RecordQueue queue, IProgressBar bar, Repository repository, ObjectId Id) {
            try {
                foreach (var records in queue) {
                    repository.Push(Id, records);
                    bar.Tick();
                }
            }
            catch (Exception e) {
                return e;
            }

            return null;
        }

        private static IEnumerable<SeedFilePair> MakeSeedFilePair(IEnumerable<string> files) {
            var rt = new List<Tuple<int, string>>();

            Spinner.Start("Generating...", spin => {
                rt.AddRange(from file in files
                    let seed = int.Parse(file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                        .Last()
                        .Substring("SEED".Length, 5))
                    select Tuple.Create(seed, file));

                spin.Info($"{rt.Count} files found");
            });

            return rt;
        }
    }
}