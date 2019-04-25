using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace taa {
    public class Counter {
        private readonly Filter Filter;
        private readonly Dictionary<string, WvCSV> signalDictionary;

        public Counter(Filter f) {
            Filter = f;
            signalDictionary = new Dictionary<string, WvCSV>();
        }

        public void AddWvCSV(string name, WvCSV csv) {
            if(signalDictionary.ContainsKey(name)) throw new Exception(
                $"Already inserted signal csv: {name}");

            signalDictionary.Add(name, csv);
        }

        public long Aggregate() {

            Filter.Build();
            var len = signalDictionary.First().Value.Length;
            if (signalDictionary.Any(x => x.Value.Length != len)) throw new Exception("長さの違うCSVがあります");


            // 数え上げ
            var result = new List<IEnumerable<bool>>();
            foreach (var target in Filter.Targets) {
                using (var lua = new CounterLua()) {
                    for (var i = 0; i < len; i++) {
                        lua.AddStatus(BuildStatement(i, target));
                    }

                    result.Add(lua.GetResults());
                }
            }

            // 答えを求めるLuaスクリプトのビルド
            // syntax
            // Answer := {V}, {S}
            // S := {V}, {V}{O}{V}, {V}{O}{S}, ({V}{O}{S}), ({V}{O}{V})
            // V := N, [0-9]+, |[0-9]+|
            // O := +, -, *, /
            var script = "return ";
            var done = "";
            foreach (var item in Filter.ParsedAnswer) {
                done += item;
                if (item == "N") script += $"{len}";
                else if (new[] {"+", "-", "*", "/", "(", ")"}.Contains(item)) script += item;
                else if (item.First() == '|') {
                    var msg = $"Failed Parse Answer Expression: {done} <= ";
                    if (item.Last() != '|') throw new Exception(msg);

                    if (int.TryParse(item.Split('|')[0], out var idx)) {
                        script += $"{result[idx]}";
                    }
                    else throw new Exception(msg + " is not integer");
                }
                else {
                    script += item;
                }
            }

            // 実行して返す
            using (var lua = new Lua()) {
                return (long) lua.DoString(script)[0];
            }
        }

        private string BuildStatement(int idx, IEnumerable<string> input) {
            var rt = "";

            foreach (var item in input) {
                if (new[] {"not", "and", "or", "(", ")"}.Contains(item)) rt += item;
                else {
                    var (s, d, c) = Filter[item];
                    rt += $"{signalDictionary[s][idx, d]}{c}";
                }

            }

            return rt;
        }

        
    }
}
