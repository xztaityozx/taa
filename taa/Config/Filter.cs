using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DynamicExpresso;
using Microsoft.EntityFrameworkCore.Internal;
using taa.Extension;
using taa.Model;

namespace taa.Config {
    using FilterFunc=Func<Map<string, decimal>, bool>;
    public class FilterDelegates {
        public readonly Func<Map<string, decimal>, bool> Filter;
        public readonly string Name;

        public FilterDelegates(string name, FilterFunc filter) {
            Filter = filter;
            Name = name;
        }
    }

    public class Filter {
        public IReadOnlyList<FilterDelegates> Delegates { get; }

        public Filter(Dictionary<string, string> conditions, IEnumerable<string> expressions) {
            var map = new Map<string, string>();
            foreach (var (key, value) in conditions) {
                map[key] = Decode(value);
            }

            filterList.AddRange(conditions.Select(x=>$"{x.Key}: {x.Value}"));
            var ops = new[] {"||", "&&", "(", ")", "!"};
            var itr = new Interpreter();
            var ds = (from item in expressions
                let exp = ops.Aggregate(item, (cur, op) => cur.Replace(op, $" {op} "))
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => map.ContainsKey(s) ? map[s] : s)
                    .Join("")
                select new FilterDelegates(item, itr.ParseAsDelegate<FilterFunc>(exp, "map"))).ToList();

            Delegates = ds;
        }

        private readonly string[] operators = { "<", ">", "<=" , ">=", "==", "!="};
        private string Decode(string cond) {
            var box = operators.Skip(2).Aggregate(cond, (current, op) => current.Replace(op, $" {op} "))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (box.Length != 3) {
                box = operators.Take(2).Aggregate(cond, (current, op) => current.Replace(op, $" {op} "))
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
            }
            
            return $"{Value(box[0])}{box[1]}{Value(box[2])}";
        }

        public Tuple<string, long>[] Aggregate(List<Map<string, decimal>> list) {
            var map = new Map<string, long>();

            foreach (var d in Delegates) {
                map[d.Name] = list.Count(d.Filter);
            }

            return map.Select(x => Tuple.Create(x.Key, x.Value)).ToArray();
        }

        private List<string> filterList = new List<string>();
        public override string ToString() {
            return string.Join("\n",filterList);
        }

        private static string Value(string value) {
            return value.TryParseDecimalWithSiUnit(out var x) ? $"{x}M" : Signal(value);
        }

        private static string Signal(string value) {
            var box = value.Split(new []{"[","]"}, StringSplitOptions.RemoveEmptyEntries);
            var signal = box[0];
            var time = box[1].ParseDecimalWithSiUnit();
            return $"map[\"{Record.EncodeKey(signal, time)}\"]";
        }
    }
}
