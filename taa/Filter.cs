using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DynamicExpresso;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace taa {
    using FilterFuncType = Func<Map<string,decimal>, bool>;
    public class Filter {
        private Map<string, FilterFuncType> delegates;
        private readonly Config config;
        public Filter(Config config) {
            this.config = config;
        }

        public IEnumerable<string> Build() {

            yield return "Loading Conditions...";

            var dic = new Dictionary<string, string>();
            foreach (var (key, value) in config.Conditions) {
                var split = value.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
                var k = Document.EncodeKey(split[0], Document.ParseDecimalWithSiUnit(split[1]));
                var op = split[2].Substring(0, 2);
                if (!new[] {"<=", ">=", "==", "!="}.Contains(op)) op = $"{split[2][0]}";
                var v = Document.ParseDecimalWithSiUnit(split[2].Replace(op, ""));
                // key = signal/time operator value
                dic.Add(key, $"(map[{k}]{op}{v}M)");
            }

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
                delegates.Add(exp,d);
            }

            yield return "Finished";
        }

        public long[] Aggregate(Document document, MultiProgressBar mpb) {
            var rt = new long[delegates.Count];

            foreach (var item in delegates.Select((pair, i) => new{n=pair.Key,d=pair.Value, i})) {
                rt[item.i] = document.Count(map => item.d(map));
                mpb.Tick(item.n);
            }

            return rt;
        }
    }
}