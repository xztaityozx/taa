using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using DynamicExpresso;
using NLua;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var dir = @"C:\Users\xztaityozx\source\repos\taa\taa\test\m8d";

            var sw = new Stopwatch();
            sw.Start();
            var r = WvCsvParser.Parse(dir, new[] { "m8d" }, 100, 5000, 10);
            sw.PrintElapsedMilliseconds("Finished: WvCsvParser.Parse(): ");
            sw.Restart();

            var config = new Config();
            config.AddCondition("A", "m8d[2.5n]>=0.4");
            config.AddCondition("B", "m8d[10n]>=0.4");
            config.AddCondition("C", "m8d[17.5n]>=0.4");

            //config.AddCondition("D", "N2[2.5n]>=0.4");
            //config.AddCondition("E", "N2[10n]>=0.4");
            //config.AddCondition("F", "N2[17.5n]>=0.4");

            config.AddExpression("A&&B&&C");

            var f = new Filter(config);
            var fs = f.Build();
            sw.PrintElapsedMilliseconds("Finished: filter.Build(): ");
            sw.Restart();
            r.Count(fs[0]).WL();
            sw.PrintElapsedMilliseconds("Finished: Record.Count(fs[0]).WL(): ");

            return;
        }
    }

    public static class Extension {
        public static void WL<T>(this IEnumerable<T> @this) {
            foreach (var t in @this) {
                Console.WriteLine(t);
            }
        }

        public static void WL(this object @this) => Console.WriteLine(@this);

        public static void PrintElapsedMilliseconds(this Stopwatch @this, string prefix="") 
            => Console.WriteLine($"{prefix}{@this.ElapsedMilliseconds}ms");
    }
}