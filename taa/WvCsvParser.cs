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

        //public static Record Parse(string dir, IReadOnlyCollection<string> signals, int seeds, int times, int parallel) {
            

        //}


        private readonly string[] elementDelimiter = {",", " "};
        private Tuple<IEnumerable<string>, IEnumerable<decimal[]>> GetElements(string file) {
            using (var sr = new StreamReader(file)) {
                var str = sr.ReadToEnd();
                var split = str.Split(elementDelimiter, StringSplitOptions.RemoveEmptyEntries);

                var signalName = split[1].Split('\n', StringSplitOptions.RemoveEmptyEntries)[1].Replace("Time ,", "");
                var elements = split.Skip(2).Select(ParseElement).ToArray();

                var keyList = elements.First().Item2.Select(time => Record.GenKeyString(signalName, time));
                var valuesList = elements.Select(t => t.Item3.ToArray());

                return Tuple.Create(keyList, valuesList);
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
