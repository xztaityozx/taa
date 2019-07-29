using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;
using Microsoft.EntityFrameworkCore.Internal;
using taa.Model;

namespace taa.Config {
    using FilterFunc = Func<Map<string, decimal>, bool>;
    public class Filter {
        
        private Dictionary<string,FilterFunc> Delegates { get; }

        public Filter(Dictionary<string, string> conditions, IEnumerable<string> expressions) {
            Delegates = new Dictionary<string, FilterFunc>();

            var map = new Map<string, string>();
            foreach (var (key, value) in conditions) {
                map[key] = Decode(value);
            }

            var ops = new[] {"||", "&&", "(", ")", "!"};
            var itr = new Interpreter();
            foreach (var item in expressions) {
                var exp = ops.Aggregate(item, (cur, op) => cur.Replace(op, $" {op} "))
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => map.ContainsKey(s) ? map[s] : s).Join("");
                Delegates.Add(item, itr.ParseAsDelegate<FilterFunc>(exp, "map"));
            }
        }

        public Tuple<string,long>[] Aggregate(Document.Document document) {
            var rt = new Map<string, long>();

            foreach (var (k,d) in Delegates) {
                rt[k] = document.Aggregate(d);
            }

            return rt.Select(k => Tuple.Create(k.Key, k.Value)).ToArray();
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


        private static string Value(string value) {
            return Document.Document.TryParseDecimalWithSiUnit(value, out var x) ? $"{x}M" : Signal(value);
        }

        private static string Signal(string value) {
            var box = value.Split(new []{"[","]"}, StringSplitOptions.RemoveEmptyEntries);
            var signal = box[0];
            var time = Document.Document.ParseDecimalWithSiUnit(box[1]);
            return $"map[\"{RecordModel.EncodeKey(signal, time)}\"]";
        }
    }
}
