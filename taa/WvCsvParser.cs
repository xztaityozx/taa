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

        public static Record Parse(string dir, IReadOnlyCollection<string> signals, int seeds, int times) {
            var rt = new Record(signals, seeds*times);
            var para = signals.Count();
            for (var i = 1; i <= seeds; i++) {
                signals.AsParallel()
                    .WithDegreeOfParallelism(para)
                    .ForAll(s => {
                        foreach (var element in GetElements(Path.Join(dir, s, $"SEED{i:D5}.csv"))) {
                            var idx = element.Index - 1 + times*(i-1);
                            foreach (var (k, v) in element.List) {
                                rt[idx, s, k] = v;
                            }
                        }
                    });
            }

            return rt;
        }

        private static IEnumerable<Element> GetElements(string file) {
            var elements = new List<Element>();
            using (var sr = new StreamReader(file)) {
                var str = sr.ReadToEnd();
                elements.AddRange(str
                    .Split("#", StringSplitOptions.RemoveEmptyEntries)
                    .Skip(2).Select(item => new Element(item)));
            }

            return elements;
        }

        public class Element {
            private readonly Map<decimal, double> valueMap;

            public Element(string str) {
                valueMap=new Map<decimal, double>();

                // str format
                // #sweep index
                // key1, value1
                // key2, value2
                // ...
                var delimiter = new[] { ',', ' ' };

                var lines = str.Split("\n",StringSplitOptions.RemoveEmptyEntries);
                Index = int.Parse(lines[0].Split(delimiter)[1]);

                foreach (var item in lines.Skip(1).Select(s => {
                    var l = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    return new {
                        k = decimal.Parse(l[0], NumberStyles.Float),
                        v = double.Parse(l[1], NumberStyles.Float)
                    };
                })) {
                    valueMap[item.k] = item.v;
                }

            }

            public int Index { get; }

            public IEnumerable<Tuple<decimal, double>> List =>
                valueMap.Select(item => Tuple.Create(item.Key, item.Value));
        }
    }
}
