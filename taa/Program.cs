using System;
using System.Collections;
using System.Collections.Generic;
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
            var csv = @"C:\Users\xztaityozx\source\repos\taa\taa\test\m8d\m8d\SEED00001.csv";

            var p = new WvParser(csv);
            var wvcsv = p.Parse();

            var f = new Filter(new Dictionary<string, string> {
                ["A"] = "m8d[2.5n]>=0.4",
                ["B"] = "m8d[10n]>=0.4",
                ["C"] = "m8d[17.5n]>=0.4"
            }, new List<string> {
                "A&&B&&C"
            }, "|1|");
            var c = new Counter(f);

            c.AddWvCSV("m8d", wvcsv);

            var ans = c.Aggregate();

            Console.WriteLine(ans);

            //            var str = @"#format 2dsweep MONTE_CARLO
            //#[Custom WaveView] saved 11:45:40 Fri Apr 12 2019
            //TIME ,m8d";
            //            foreach (var s in str.Split("#")) {
            //                Console.WriteLine(s);
            //            }

        }

    }
}
