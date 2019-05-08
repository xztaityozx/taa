using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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


        public Filter(Dictionary<string, string> d, List<string> t, string ans) {
            ConditionList = d;
            TargetList = t;
            Answer = ans;
            
            Build();
        }

        public void Build() {
            BuildCondition();
            BuildTarget();
            ParseAnswer();
        }

        private void BuildTarget() {
            targetExpressionList = new List<string[]>();
            foreach (var item in TargetList) {
                targetExpressionList.Add(item.Replace("&&", " and ")
                    .Replace("||", " or ")
                    .Replace("!"," not ")
                    .Replace("("," ( ")
                    .Replace(")"," ) ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public IEnumerable<string> ParsedAnswer { get; private set; }
        private void ParseAnswer() {
            ParsedAnswer = new[] {"+", "-", "*", "/", "(", ")"}
                .Aggregate(Answer, (current, ope) => current.Replace(ope, $" {ope} "))
                .Split(' ',StringSplitOptions.RemoveEmptyEntries);
        }

        private void BuildCondition() {
            conditionDictionary=new Dictionary<string, Tuple<string, decimal, string>>();
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
                var time = ParseDecimalWithSiUnit(box[1]);
                var ope = box[2][1] == '=' ? box[2].Substring(0, 2) : $"{box[2][0]}";
                var value = ParseDecimalWithSiUnit(box[2].Substring(ope.Length, box[2].Length - ope.Length));

                conditionDictionary.Add(key, new Tuple<string, decimal, string>(signal, time, $"{ope}{value}"));

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

        private Dictionary<string, Tuple<string, decimal, string>> conditionDictionary;
        private List<string[]> targetExpressionList;

        public IReadOnlyList<string[]> Targets => targetExpressionList;
        
        public Tuple<string, decimal, string> this[string name] {
            get {
                var (s, d, c) = conditionDictionary[name];
                return Tuple.Create(s, d, c);
            }
        }
    }

}