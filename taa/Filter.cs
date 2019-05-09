﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        [YamlMember(Alias = "targets")]
        public List<string> TargetList { get; }

        /// <summary>
        /// 答え
        /// </summary>
        [YamlMember(Alias = "answer")]
        public string Answer { get; }
    }

    public class Filter {
        private readonly Config config;

        public Filter(Config config) {
            this.config = config;
            variables = new Map<string, Tuple<string, decimal>>();
            expressionMap = new Map<string, Func<decimal, bool>>();
            targetExpressionList = new List<string[]>();
            answerExpression=new List<string>();

            Build();
        }

        private readonly Map<string, Tuple<string, decimal>> variables;
        private readonly Map<string, Func<decimal, bool>> expressionMap;

        private readonly List<string[]> targetExpressionList;
        private readonly List<string> answerExpression;

        private void Build() {
            foreach (var (k, v) in config.ConditionList) {
                var (s, t, f) = ParseExpression(v);
                variables[k] = Tuple.Create(s, t);
                expressionMap[k] = f;
            }

            foreach (var item in config.TargetList) {
                targetExpressionList.Add(item
                    .Replace("&&", " and ")
                    .Replace("||", " or ")
                    .Replace("!", " not ")
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
            
            answerExpression.AddRange(
                new[] {"+", "-", "*", "/", "(", ")"}
                .Aggregate(config.Answer, (current, ope) => current.Replace(ope, $" {ope} "))
                .Split(' '));
        }

        private readonly char[] delimiter = {'[', ']'};
        private readonly string[] expectOperator = {"<", "<=", ">", ">=", "!=", "=="};

        private Tuple<string, decimal, Func<decimal, bool>> ParseExpression(string exp) {
            // exp := {SignalName}[{Time}{SiUnit}*]{Operator}{Value{SiUnit}*}
            // SignalName := [a-zA-Z][a-zA-Z0-9]*
            // Time := [0-9]+{.[0-9]+}?
            // SiUnit := G,M,k,m,n,u,p
            // Operator  := <,<=,>,>=,==,!=
            var split = exp.Replace(" ", "").Split(delimiter);
            var signalName = split[0];
            var time = WvCsvParser.ParseDecimalWithSiUnit(split[1]);
            Func<decimal, bool> expression;

            {
                // 演算子切り出し
                string op;
                if (expectOperator.Contains(split[2].Substring(0, 2))) op = split[2].Substring(0, 2);
                else if (expectOperator.Contains(split[2].Substring(0, 1))) op = $"{split[2][0]}";
                else {
                    throw new Exception($"[Taa] Invalid Operator: {exp}");
                }

                var v = WvCsvParser.ParseDecimalWithSiUnit(split[2].Replace(op, ""));


                if (op == expectOperator[0]) expression = d => d < v;
                else if (op == expectOperator[1]) expression = d => d <= v;
                else if (op == expectOperator[2]) expression = d => d > v;
                else if (op == expectOperator[3]) expression = d => d >= v;
                else if (op == expectOperator[4]) expression = d => d != v;
                else expression = d => d == v;
            }

            return Tuple.Create(signalName, time, expression);
        }
    }
}