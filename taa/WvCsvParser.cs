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

        public static Record Parse(string dir, IReadOnlyCollection<string> signals, int seeds, int times, int parallel) {
            var rt = new Record(signals, seeds*times);
            
            var list = new List<Tuple<int,string,string>>();

            for (var i = 1; i <= seeds; i++) {
                list.AddRange(signals
                    .Select(signal =>Tuple.Create(i, signal, Path.Join(dir, signal, $"SEED{i:D5}.csv"))));
            }

            list.AsParallel().WithDegreeOfParallelism(parallel).ForAll(s => {
                foreach (var element in GetElements(s.Item3)) {
                    var idx = element.Index - 1 + times * (s.Item1 - 1);
                        foreach (var (k, v) in element.List) {
                            rt[idx, s.Item2, k] = v;
                        }
                }
            });

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
            private readonly Map<decimal, decimal> valueMap;

            public Element(string str) {
                valueMap=new Map<decimal, decimal>();

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
                        v = decimal.Parse(l[1], NumberStyles.Float)
                    };
                })) {
                    valueMap[item.k] = item.v;
                }

            }

            public int Index { get; }

            public IEnumerable<Tuple<decimal, decimal>> List =>
                valueMap.Select(item => Tuple.Create(item.Key, item.Value));
        }
    }
}
