using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using DynamicExpresso;
using NLua;
using ShellProgressBar;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var dir = @"/home/xztaityozx/TestDir";
//            var dir = @"C:\Users\xztaityozx\source\repos\taa\taa\test\m8d";

            var config = new Config(10, new[] {"m8d"}, 5000, 2000);
            config.AddCondition("A", "m8d[2.5n]>=0.4");
            config.AddCondition("B", "m8d[10n]>=0.4");
            config.AddCondition("C", "m8d[17.5n]>=0.4");

            config.AddExpression("A&&B&&C");
            config.AddExpression("A&&B&&C");
            config.AddExpression("A&&B&&C");
            config.AddExpression("A&&B&&C");
            config.AddExpression("A&&B&&C");

            var cts =new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            var d = new Dispatcher(config);

            var res = d.Start(cts.Token, dir);

            res.WL();


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

        public static void PrintFinish(this Stopwatch @this, string message)
            => @this.PrintElapsedMilliseconds($"Finished: {message}");
    }
}