using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using taa.Model;

namespace taa.Document {
    public class Document {
        public Parameter.Parameter Parameter { get; }
        private readonly Map<long, Map<string, decimal>> map;
        public int Size => map.Count;

        public Document(Parameter.Parameter parameter, IEnumerable<RecordModel> records, long sweeps) {
            map = new Map<long, Map<string, decimal>>();
            for (var i = 0L; i < sweeps; i++) {
                map[i+1] = new Map<string, decimal>();
            }

            foreach (var record in records) {
                map[record.Sweep][record.Key] = record.Value;
            }

            Parameter = parameter;
        }

        public long Aggregate(Func<Map<string, decimal>, bool> filter) => map.Count(item => filter(item.Value));

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
        public static bool TryParseDecimalWithSiUnit(string s, out decimal v) {
            v = default;
            try {
                v = ParseDecimalWithSiUnit(s);
            }
            catch (FormatException) {
                return false;
            }
            return true;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (var (idx, m) in map) {
                sb.AppendLine($"{idx} {m.Select(k => $"key:{k.Key},value:{k.Value}").Join(" ")}");
            }

            return sb.ToString();
        }
    }
}