using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace taa {
    public class Document :IEnumerable<Map<string,decimal>> {
        private readonly List<Map<string, decimal>> dataMap;
        public int Seed { get; }
        public int Sweeps { get; }
        private readonly char[] delimiter = {' ', ','};

        public List<string> KeyList;

        public Document(int seed, int sweeps) {
            dataMap = new List<Map<string, decimal>>();
            for (var i = 0; i < sweeps; i++) {
                dataMap.Add(new Map<string, decimal>());
            }

            Seed = seed;
            Sweeps = sweeps;
            KeyList = new List<string>();
        }

        public void Append(string key, decimal[] values) {
            for (var i = 0; i < Sweeps; i++) {
                dataMap[i][key] = values[i];
            }
            KeyList.Add(key);
        }


        public Document(string file, int seed, int sweeps) :this(seed,sweeps) {
            string str;
            using (var sr = new StreamReader(file)) str = sr.ReadToEnd();

            var signals = new List<string>();
            var indexes = new Map<decimal, int>();

            foreach (var line in str.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries))) {
                if (line.All(string.IsNullOrEmpty)) continue;
                if (line[0][0] == '#') continue;
                if (line[0] == "TIME") {
                    signals = line.Skip(1).ToList();
                }
                else {
                    var time = decimal.Parse(line[0], NumberStyles.Float);
                    var values = line.Skip(1).Select(d => decimal.Parse(d, NumberStyles.Float)).ToArray();

                    for (var i = 0; i < signals.Count; i++) {
                        var key = EncodeKey(signals[i], time);
                        KeyList.Add(key);
                        dataMap[indexes[time]][key] = values[i];
                    }

                    indexes[time]++;
                }
            }

            KeyList = KeyList.Distinct().ToList();

            if (indexes.Any(i => i.Value != Sweeps))
                throw new Exception($"データの長さがおかしいです: {string.Join(", ", indexes.Select(i => i.Value))}");
        }

        public static string EncodeKey(string s, decimal t) => $"{s}/{t:E10}";

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

        public IEnumerator<Map<string, decimal>> GetEnumerator() {
            return dataMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// DocumentからRecord[]を作る
        /// </summary>
        /// <param name="vtn"></param>
        /// <param name="vtp"></param>
        /// <returns></returns>
        public IEnumerable<Record> GenerateRecords(Transistor vtn, Transistor vtp) {
            var box = new Map<string, List<decimal>>();

            foreach (var k in KeyList) {
                box[k] = new List<decimal>();
            }

            foreach (var map in dataMap) {
                foreach (var (k,v) in map) {
                    box[k].Add(v);
                }
            }

            return KeyList.Select(k => new Record {
                Vtn = vtn,
                Vtp = vtp,
                Key = k,
                Seed = Seed,
                Sweeps = Sweeps,
                Values = box[k].ToArray()
            });
        }

        /// <summary>
        /// Record[]からDocument[]を生成する。Vtn,Vtpの組み合わせがすべて一緒でないといけない
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        public static IEnumerable<Document> BuildDocuments(IEnumerable<Record> records) {
            var box = records as Record[] ?? records.ToArray();
            if (box.Select(r => new {n = r.Vtn, p = r.Vtp}).Distinct().Count() != 1)
                throw new Exception("Vtn,Vtpの組み合わせが複数あります");

            var map = new Map<int, Document>();
            foreach (var record in box) {
                if (map[record.Seed] == null) map[record.Seed] = new Document(record.Seed, record.Sweeps);
                map[record.Seed].Append(record.Key, record.Values);
            }

            return map.Select(m => m.Value);
        }
        
    }
}