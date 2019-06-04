using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kurukuru;
using YamlDotNet.Serialization;

namespace taa {
    public class Dispatcher {
        public long[] Dispatch(CancellationToken token, Request request, Config config) {
            var expressionCount = config.Expressions.Count;
            var rt = new long[expressionCount];
            var filter = new Filter(config);

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



            return rt;
        }
    }
}