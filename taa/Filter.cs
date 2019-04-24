using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace taa {
    public class Filter {
        /// <summary>
        /// 評価式のリスト
        /// </summary>
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> ConditionList { get; }
        /// <summary>
        /// 数え上げ対象の評価式のリスト
        /// </summary>
        [YamlMember(Alias = "targets")]
        public List<string> TargetList { get; }

        /// <summary>
        /// 答え
        /// </summary>
        [YamlMember(Alias = "answer")]
        public string Answer { get; }

        public Filter() {
            ConditionList=new Dictionary<string, string>();
            TargetList=new List<string>();
        }

        public Filter(Dictionary<string, string> d, List<string> t) {
            ConditionList = d;
            TargetList = t;
        }

        public void Build() {
            actionDictionary = new Dictionary<string, Dictionary<decimal, Func<decimal, bool>>>();
            conditionDictionary=new Dictionary<string, Tuple<string, decimal>>();
            
            BuildCondition();

            BuildTarget();
        }

        private void BuildTarget() {
            targetExpressionList = new List<string[]>();
            foreach (var item in TargetList) {
                targetExpressionList.Add(item.Replace("&&", " && ").Replace("||", " || ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void BuildCondition() {
            foreach (var (key, v) in ConditionList) {
                // cond format
                // {Signal}[{Time}{SIUnit}?]{Operator}{Value}{SIUnit}?
                // Signal := ([a-zA-Z0-9])+
                // Time := [0-9]+(.[0-9]+)*
                // SIUnit := G|M|K|m|u|n|p
                // Operator := <|<=|>|>=|==|!=
                // Value := [0-9]+(.[0-9]+)*

                var box = v.Trim(' ').Split('[', ']');
                var signal = box[0];
                var time = ParseSIUnit(box[1]);
                var ope = box[2][1] == '=' ? box[2].Substring(0, 2) : $"{box[2][0]}";
                var value = ParseSIUnit(box[2].Substring(ope.Length, box[2].Length - ope.Length));

                if (!actionDictionary.ContainsKey(signal)) actionDictionary.Add(signal, new Dictionary<decimal, Func<decimal, bool>>());
                actionDictionary[signal].Add(time, MakeFunc(ope, value));

                if (conditionDictionary.ContainsKey(signal)) throw new Exception($"{signal} already declared");
                conditionDictionary.Add(key, new Tuple<string, decimal>(signal, time));
            }
        }

        private static Func<decimal, bool> MakeFunc(string ope, decimal val) {
            if (ope == "<") return (d) => d < val;
            if (ope == "<=") return (d) => d <= val;
            if (ope == ">") return (d) => d > val;
            if (ope == ">=") return (d) => d >= val;
            if (ope == "==") return (d) => d == val;
            if (ope == "!=") return (d) => d != val;

            throw new Exception($"Unexpected operator: {ope}");
        }

        public static decimal ParseSIUnit(string s) {
            decimal Func(string input) {
                if (decimal.TryParse(input, out _)) return 1M;
                if (input == "G") return 1e9M;
                if (input == "M") return 1e6M;
                if (input == "K") return 1e3M;
                if (input == "m") return 1e-3M;
                if (input == "u") return 1e-6M;
                if (input == "n") return 1e-9M;
                if (input == "p") return 1e-12M;

                throw new Exception($"Unexpected SI Unit: {input}");
            }

            var unit = Func($"{s.Last()}");
            if (unit != 1M) {
                s = s.Substring(0, s.Length - 1);
            }

            return decimal.Parse(s) * unit;

        }
        private Dictionary<string, Dictionary<decimal, Func<decimal, bool>>> actionDictionary;
        private Dictionary<string, Tuple<string, decimal>> conditionDictionary;
        private List<string[]> targetExpressionList;

        public Tuple<string,decimal> this[string name] => conditionDictionary[name];
        public Func<decimal, bool> this[string signal, decimal time] => actionDictionary[signal][time];
        public string[] this[int idx] => targetExpressionList[idx];
    }

}