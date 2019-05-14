using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DynamicExpresso;
using YamlDotNet.Serialization;

namespace taa {
    public struct Config {
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
    }

    public class Filter {
        private Config config;
        private readonly char[] delimiter = {'[', ']'};

        public Filter(Config c) => config = c;

        public IEnumerable<Func<Map<string, decimal>, bool>> Build() {

            var conditionMap = new Map<string, string>();
            foreach (var (key,value) in config.ConditionList) {
                var split = value.Trim(' ').Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                
                if(split.Length<3) throw new Exception($"Invalid Condition: {key}: {value} <=");
                
                var sig = split[0];
                var time = WvCsvParser.ParseDecimalWithSiUnit(split[1]);
                
                conditionMap[key] = $"map[\"{Record.GetKey(sig, time)}\"]{split[2]}";
            }
            
            var itr = new Interpreter();

            var rt = config.TargetList.Select(item => {
                return string.Join("", item
                    .Replace("&&", " && ")
                    .Replace("||", " || ")
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Replace("!", " ! ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s =>
                        s == "&&" || s == "||" || s == "!" || s == "(" || s == ")" ? s : $"({conditionMap[s]})"));
            }).Select(exp => itr.ParseAsDelegate<Func<Map<string, decimal>, bool>>(exp));
                
            

            return rt;
        }

    }
}