using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DynamicExpresso;
using ShellProgressBar;
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

        /// <summary>
        /// 並列数
        /// </summary>
        [YamlMember(Alias="parallel")]
        public int Parallel { get; }

        [YamlMember(Alias = "signals")]
        public IReadOnlyList<string> Signals { get; }

        [YamlMember(Alias = "times")]
        public int Times { get; }

        [YamlMember(Alias = "range")]
        public int Range { get; }

        public Config(int p, IReadOnlyList<string> s, int time, int range) {
            ConditionList=new Dictionary<string, string>();
            ExpressionList=new List<string>();
            Parallel = p;
            Signals = s;
            Times = time;
            Range = range;
        }
        public void AddCondition(string key, string value) => ConditionList.Add(key, value);
        public void AddExpression(string target) => ExpressionList.Add(target);
    }

    public class Filter {
        private readonly Config config;
        private readonly char[] delimiter = {'[', ']'};

        public Filter(Config c) => config = c;

        public List<Func<Map<string, decimal>, bool>> Build(ChildProgressBar parent) {
            var rt = new List<Func<Map<string, decimal>, bool>>();

            var conditionMap = new Map<string, string> {
                ["&&"] = "&&",
                ["||"] = "||",
                ["!"] = "!",
                ["("] = "(",
                [")"] = ")"
            };

            var cOption = new ProgressBarOptions {
                ProgressCharacter = '-',
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = false
            };

            // build condition map
            using (var pb =
                parent.Spawn(config.ConditionList.Count, "Build conditions", cOption)) {

                var idx = 0;
                foreach (var (key,value) in config.ConditionList) {

                    var split = value.Trim(' ').Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    var sig = split[0];
                    var time = Record.ParseDecimalWithSiUnit(split[1]);
                    var ope = split[2][1] == '=' ? split[2].Substring(0, 2) : $"{split[2][0]}";
                    var x = Record.ParseDecimalWithSiUnit(split[2].Substring(ope.Length,
                        split[2].Length - ope.Length));


                    conditionMap[key] = $"map[\"{Record.GetKey(sig, time)}\"]{ope}{x}M";
                    pb.Tick($"Finished: {idx+1}/{config.ConditionList.Count}");
                    idx++;
                }

            }

            parent.Tick("[Done]: Build Condition List");

            // parse delegate
            using (var pb = parent.Spawn(config.ExpressionList.Count, "Build Delegates", cOption)) {
                var idx = 0;
                var itr=new Interpreter();
                foreach (var exp in config.ExpressionList
                    .Select(item=> string.Join("", item
                        .Replace("&&", " && ")
                        .Replace("||", " || ")
                        .Replace("(", " ( ")
                        .Replace(")", " ) ")
                        .Replace("!", " ! ")
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => conditionMap[s])))) {
                    
                    rt.Add(itr.ParseAsDelegate<Func<Map<string,decimal>,bool>>(exp,"map"));
                    pb.Tick($"Finished: {idx+1}/{config.ExpressionList.Count}");
                    idx++;
                }
            }

            parent.Tick("[Done]: Build Delegates");

            return rt;
        }

    }
}