using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa {
    public class Aggregator {
        public static decimal ParseDecimalWithSiUnit(string s) {
            return decimal.Parse(s.Replace("G", "E09")
                    .Replace("M", "E06")
                    .Replace("K", "E03")
                    .Replace("m", "E-03")
                    .Replace("u", "E-06")
                    .Replace("n", "E-09")
                    .Replace("p", "E-12"),
                NumberStyles.Float);
        }

        private readonly string dir;
        private readonly IReadOnlyCollection<string> signals;
        private readonly int times;

        private readonly Record record;

        private Record JoinFiles(int seed) {
            var rt = new Record(times);

            foreach (var s in signals) {
                var path = Path.Join(dir, s, $"SEED{seed:D5}.csv");
                var d = new Document(path);

                foreach (var (k, v) in d.DataMap) {
                    var length = v.Count;
                    for (var i = 0; i < length; i++) {
                        rt[i, s, k] = v[i];
                    }
                }
            }

            return rt;
        }

        public Aggregator(string dir, IReadOnlyCollection<string> signals, int times, int seed) {
            this.dir = dir;
            this.signals = signals;
            this.times = times;
            record = JoinFiles(seed);
        }

        public long Aggregate(Func<Map<string,decimal>, bool> filter) {
            return record.Count(filter);
        }

        private class Document {

            private readonly char[] delimiter = {' ', ','};

            public Dictionary<decimal, List<decimal>> DataMap { get; }

            public Document(string path) {
                string str;
                using (var sr = new StreamReader(path)) {
                    str = sr.ReadToEnd();
                }

                DataMap=new Dictionary<decimal, List<decimal>>();

                foreach (var tv in str.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(l => !string.IsNullOrEmpty(l) && l[0]!='#' && l[0]!='T')
                    .Select(item=>item.Split(delimiter,StringSplitOptions.RemoveEmptyEntries))
                    .Select(a=>new {t=decimal.Parse(a[0],NumberStyles.Float), v=decimal.Parse(a[1],NumberStyles.Float)})) {

                    if (!DataMap.ContainsKey(tv.t))  DataMap[tv.t]=new List<decimal>();
                    
                    DataMap[tv.t].Add(tv.v);
                    
                }

            }


        }

    }
}
