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
            //var dir = @"/home/xztaityozx/TestDir";
            const string dir = @"C:\Users\xztaityozx\source\repos\taa\taa\test\m8d";

            Action act = () => {

                var config = new Config(5, new[] {"m8d"}, 5000, 100);
                //config.AddCondition("A", "N1[2.5n]>=0.4");
                //config.AddCondition("B", "N1[10n]>=0.4");
                //config.AddCondition("C", "N1[17.5n]>=0.4");

                //config.AddCondition("D", "N1[2.5n]>=0.4");
                //config.AddCondition("E", "N1[10n]>=0.4");
                //config.AddCondition("F", "N1[17.5n]>=0.4");


                //config.AddExpression("A&&B&&C&&D&&E&&F");
                //config.AddExpression("A&&!(E)");
                //config.AddExpression("D");
                //config.AddExpression("A");

                config.AddCondition("A", "m8d[2.5n]>=0.4");
                config.AddCondition("B", "m8d[10n]>=0.4");
                config.AddCondition("C", "m8d[17.5n]>=0.4");

                config.AddExpression("A&&B&&C");
                config.AddExpression("A&&B");
                config.AddExpression("C");
                config.AddExpression("A");


                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) => {
                    e.Cancel = true;
                    cts.Cancel();
                };

                var d = new Dispatcher(config);

                var res = d.Start(cts.Token, dir);

                res.WL();

            };

            act();
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