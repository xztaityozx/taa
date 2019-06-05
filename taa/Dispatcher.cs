using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kurukuru;
using ShellProgressBar;

namespace taa {
    public class Dispatcher {
        private readonly Config config;

        public Dispatcher(Config config) => this.config = config;

        public Tuple<string,long>[] Dispatch(CancellationToken token, Request request, Filter filter) {
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


            return filter.ExpressionStringList.Zip(rt, Tuple.Create).ToArray();
        }
    }
}