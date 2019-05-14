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

        // TODO: 遅すぎて使いもんにならん
        public static Record Parse(string dir, IReadOnlyCollection<string> signals, int seeds, int times, int parallel) {
            var fileList = signals.SelectMany(signal =>
                Enumerable.Range(1, seeds).Select(seed => Tuple.Create(seed, Path.Join(dir, signal, $"SEED{seed:D5}.csv"))));

            var res = fileList.AsParallel().WithDegreeOfParallelism(parallel)
                .Select(t => new Document(t.Item2, t.Item1)).ToList();

            var rt = new Record(seeds,times);
            
            foreach (var document in res) {
                var index = 0;
                var timeSet = document.TimeSet;
                var seed = document.Seed;
                var size = document.Size;
                var signal = document.Name;
                foreach (var values in document.Data) {
                    for (var i = 0; i < timeSet.Length; i++) {
                        var time = timeSet[i];
                        var value = values[i];

                        rt[index + (seed - 1) * size, signal, time] = value;
                    } 
                    index++;
                }
            }

            return rt;
        }

        private class Document {
            public decimal[] TimeSet { get; }
            public IEnumerable<decimal[]> Data { get; }
            public int Seed { get; }
            public string Name { get; }

            public int Size => Data.Count();

            private readonly char[] delimiter = {' ', ','};

            public Document(string path, int seed) {
                Seed = seed;

                var sb = new StringBuilder();
                using (var sr = new StreamReader(path)) {
                    while(sr.Peek()>0) sb.AppendLine(sr.ReadLine());
                }

                var doc = sb.ToString()
                    .Replace("\r","")
                    .Split('#',StringSplitOptions.RemoveEmptyEntries);
                Name = doc[1].Split('\n')[1].Replace("TIME ,", "").Replace(" ","");

                var res = doc.Skip(2)
                    .Select(s => s.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    .Select(ParseElement)
                    .ToList();

                TimeSet = res[0].Item2.ToArray();
                Data = res.OrderBy(e => e.Item1).Select(e => e.Item3.ToArray());
            }

            /// <summary>
            /// Tuple => Time, Data
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            private Tuple<int, IEnumerable<decimal>, IEnumerable<decimal>> ParseElement(string[] element) {
                var timeSet = new SortedSet<decimal>();
                var dataList = new List<decimal>();
                var index = int.Parse(element[0].Replace("sweep ", ""));

                foreach (var line in element.Skip(1).Select(l => l.Split(delimiter,StringSplitOptions.RemoveEmptyEntries))) {
                    var time = decimal.Parse(line[0],NumberStyles.Float);
                    var value = decimal.Parse(line[1],NumberStyles.Float);

                    timeSet.Add(time);
                    dataList.Add(value);
                }

                return new Tuple<int,IEnumerable<decimal>, IEnumerable<decimal>>(index,timeSet, dataList);
            }
        }

    }
}
