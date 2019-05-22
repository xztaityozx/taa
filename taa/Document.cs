﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace taa {
    public class Document : IEnumerable<Map<string, decimal>> {
        private readonly List<Map<string, decimal>> dataMap;
        public int Seed { get; }
        private readonly char[] delimiter = {' ', ','};

        public Document(string file, int times, int seed) {
            dataMap = new List<Map<string, decimal>>();
            for (var i = 0; i < times; i++) {
                dataMap.Add(new Map<string, decimal>());
            }

            Seed = seed;


            string doc;
            using (var sr = new StreamReader(file)) {
                doc = sr.ReadToEnd();
            }

            var signals = new List<string>();
            var indexes = new Map<decimal, int>();

            foreach (var line in doc.Split('\n', StringSplitOptions.RemoveEmptyEntries)
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
                        dataMap[indexes[time]][GetKey(signals[i], time)] = values[i];
                    }

                    indexes[time]++;
                }
            }
        }

        public static string GetKey(string signal, decimal time) => $"{signal}-{time:E10}";

        public IEnumerator<Map<string, decimal>> GetEnumerator() {
            return dataMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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