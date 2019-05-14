using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace taa {
    public class WvCsvParser {
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

        public Record Parse(string dir, IReadOnlyCollection<string> signals, int seeds, int times) {
            var res = signals
                .SelectMany(sig => Enumerable.Range(1, seeds)
                    .Select(seed => Tuple.Create(seed,Path.Join(dir, sig, $"SEED{seed:D5}.csv"))))
                .AsParallel().WithDegreeOfParallelism(signals.Count)
                .Select(f => new Document(f.Item2,f.Item1))
                .ToList();

            var keyList = res.Select(d => Tuple.Create(d.Name, d.TimeSet));
            var rt = new Record(seeds,times,keyList);

            foreach (var document in res) {
                var idx = 0;
                var timeSet = document.TimeSet.ToArray();
                var sig = document.Name;
                foreach (var values in document.Values) {
                    for (var i = 0; i < values.Length; i++) {
                        rt[times * seeds + idx, sig, timeSet[i]] = values[i];
                    }

                    idx++;
                }
            }

            return rt;
        }

        public class Document {
            public IEnumerable<decimal> TimeSet { get; }
            public IEnumerable<decimal[]> Values { get; }

            public string Name { get; }
            public int Seed { get; }
            private readonly string[] elementDelimiter = { ",", " " };

            public Document(string file, int seed) {
                Seed = seed;
                using (var sr = new StreamReader(file)) {
                    var str = sr.ReadToEnd();
                    var split = str.Split(elementDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    Name = split[1].Split('\n')[0].Replace("TIME ,", "");

                    var elements = split.Skip(2).Select(ParseElement).ToArray();

                    TimeSet = elements.First().Item2;
                    Values = elements.Select(t => t.Item3.ToArray());
                }
            }
        }

        private static Tuple<int, IEnumerable<decimal>, IEnumerable<decimal>> ParseElement(string str) {
            var timeList = new List<decimal>();
            var valueList = new List<decimal>();


            // str format
            // #sweep index
            // key1, value1
            // key2, value2
            // ...
            var delimiter = new[] {',', ' '};

            var lines = str.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            var index = int.Parse(lines[0].Split(delimiter)[1]);

            foreach (var item in lines.Skip(1).Select(s => {
                var l = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                return new {
                    k = decimal.Parse(l[0], NumberStyles.Float),
                    v = decimal.Parse(l[1], NumberStyles.Float)
                };
            })) {
                timeList.Add(item.k);
                valueList.Add(item.v);
            }

            return new Tuple<int, IEnumerable<decimal>, IEnumerable<decimal>>(index, timeList, valueList);
        }

    }
}
