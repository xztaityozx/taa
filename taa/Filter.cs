using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;

namespace taa {
    using FilterFuncType = Func<Map<string,decimal>, bool>;
    public class Filter {
        private Map<string, FilterFuncType> delegates;
        public IEnumerable<string> ExpressionStringList => config.Expressions;
        public IReadOnlyList<string> KeyList;
        private readonly Config config;
        public Filter(Config config) {
            this.config = config;
        }

        public IEnumerable<string> Build() {
            var kl =new List<string>();
            
            yield return "Loading Conditions...";

            var dic = new Dictionary<string, string>();
            foreach (var (key, value) in config.Conditions) {
                var split = value.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
                var k = Document.EncodeKey(split[0], Document.ParseDecimalWithSiUnit(split[1]));
                kl.Add(k);
                var op = split[2].Substring(0, 2);
                if (!new[] {"<=", ">=", "==", "!="}.Contains(op)) op = $"{split[2][0]}";
                var v = Document.ParseDecimalWithSiUnit(split[2].Replace(op, ""));
                // key = signal/time operator value
                dic.Add(key, $"(map[\"{k}\"]{op}{v}M)");
            }

            KeyList = kl;

            yield return "Generating Delegates...";

            delegates = new Map<string, FilterFuncType>();
            foreach (var value in config.Expressions) {
                var exp = string.Join("", value.Replace("&&", " && ")
                    .Replace("||", " || ")
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Replace("!", " ! ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => dic.ContainsKey(s) ? dic[s] : s));

                var itr = new Interpreter();
                var d = itr.ParseAsDelegate<FilterFuncType>(exp, "map");
                delegates.Add(value,d);
            }

            yield return "Finished";
        }

        public long[] Aggregate(Document document) {
            var rt = new long[delegates.Count];

            foreach (var item in delegates.Select((pair, i) => new{n=pair.Key,d=pair.Value, i})) {
                rt[item.i] = document.Count(map => item.d(map));
            }

            return rt;
        }
    }
}
