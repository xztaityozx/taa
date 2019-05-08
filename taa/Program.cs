using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using NLua;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var f = new Filter(new Dictionary<string, string> {
                ["A"] = "m8d[2.5n]>=0.4",
                ["B"] = "m8d[10n]>=0.4",
                ["C"] = "m8d[17.5n]>=0.4"
            }, new List<string> {
                "A&&B&&C"
            }, "|1|");
            Func<int, long> F = (x) => {
                var path = $"/home/xztaityozx/TestDir/m8d/SEED{x:D5}.csv";
                var csv = new WvParser(path).Parse();

                var c = new Counter(f);
                c.UpdateWvCSV("m8d", csv);

//                Console.Error.WriteLine($"Done: {path}");

                return c.Aggregate();
            };


            var ans = Enumerable.Range(1, 2000).AsParallel().WithDegreeOfParallelism(20).Select(x => F(x)).Sum();
//            var ans = F(1);
            Console.WriteLine(ans);
        }
    }
}