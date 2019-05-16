using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace taa {
    public class Record : IEnumerable<Map<string, decimal>> {
        private readonly List<Map<string, decimal>> dataMap;
        public int Seed { get; }

        public Record(string dir, IEnumerable<string> signals, int times, int seed) {
            dataMap=new List<Map<string, decimal>>();
            for (var i = 0; i < times; i++) {
                dataMap.Add(new Map<string, decimal>());
            }

            Seed = seed;
            
            
            foreach (var s in signals) {
                var path = Path.Join(dir, s, $"SEED{seed:D5}.csv");
                var d = new Document(path);

                foreach (var (k, v) in d.DataMap) {
                    var length = v.Count;
                    for (var i = 0; i < length; i++) {
                        dataMap[i][GetKey(s, k)] = v[i];
                    }
                }
            }
        }

        public long Aggregate(Func<Map<string, decimal>, bool> filter) => this.Count(filter);

        public decimal this[int index, string signal, decimal time] {
            set => dataMap[index][GetKey(signal, time)] = value;
        }

        public static string GetKey(string signal, decimal time) => $"{signal}-{time:E10}";

        public IEnumerator<Map<string, decimal>> GetEnumerator() {
            return dataMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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
    }
}