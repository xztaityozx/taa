using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DynamicExpresso;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace taa {

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
                    var time = Document.ParseDecimalWithSiUnit(split[1]);
                    var ope = split[2][1] == '=' ? split[2].Substring(0, 2) : $"{split[2][0]}";
                    var x = Document.ParseDecimalWithSiUnit(split[2].Substring(ope.Length,
                        split[2].Length - ope.Length));


                    conditionMap[key] = $"map[\"{Document.GetKey(sig, time)}\"]{ope}{x}M";
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