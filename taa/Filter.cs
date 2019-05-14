using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DynamicExpresso;
using YamlDotNet.Serialization;

namespace taa {
    public class Config {
        /// <summary>
        /// 評価式のリスト
        /// </summary>
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> ConditionList { get; }

        /// <summary>
        /// 数え上げ対象の評価式のリスト
        /// </summary>
        [YamlMember(Alias = "expressions")]
        public List<string> ExpressionList { get; }

        public Config() {
            ConditionList=new Dictionary<string, string>();
            ExpressionList=new List<string>();
        }
        public void AddCondition(string key, string value) => ConditionList.Add(key, value);
        public void AddExpression(string target) => ExpressionList.Add(target);
    }

    public class Filter {
        private readonly Config config;
        private readonly char[] delimiter = {'[', ']'};

        public Filter(Config c) => config = c;

        public List<Func<Map<string, decimal>, bool>> Build() {

            var conditionMap = new Map<string, string> {
                ["&&"] = "&&",
                ["||"] = "||",
                ["!"] = "!",
                ["("] = "(",
                [")"] = ")"
            };
            foreach (var (key,value) in config.ConditionList) {
                var split = value.Trim(' ').Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                
                if(split.Length<3) throw new Exception($"Invalid Condition: {key}: {value} <=");
                
                var sig = split[0];
                var time = WvCsvParser.ParseDecimalWithSiUnit(split[1]);
                var ope = split[2][1] == '=' ? split[2].Substring(0, 2) : $"{split[2][0]}";
                var x = WvCsvParser.ParseDecimalWithSiUnit(split[2].Substring(ope.Length,
                    split[2].Length - ope.Length));


                conditionMap[key] = $"map[\"{Record.GetKey(sig, time)}\"]{ope}{x}M";

                Console.WriteLine(conditionMap[key]);
            }
            
            var itr = new Interpreter();

            var rt = config
                .ExpressionList
                .Select(item => string.Join("", item
                    .Replace("&&", " && ")
                    .Replace("||", " || ")
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Replace("!", " ! ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => conditionMap[s]))
            ).Select(exp => itr.ParseAsDelegate<Func<Map<string, decimal>, bool>>(exp, "map"));

            return rt.ToList();
        }

    }
}