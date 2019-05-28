using System;
using System.Collections.Generic;
using CommandLine;
using Logger;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
//            var param = "push --host 150.89.227.97 --port 28001 /home/xztaityozx/TestDir/m8d/SEED00001.csv".Split(' ');
            var param = "pull --host 150.89.227.97 --port 28001 --sweeps 5000 m8d[2.5n] m8d[10n] m8d[17.5n]".Split(' ');


            var logger = new Logger.Logger(new ConsoleLogger());
            Parser.Default.ParseArguments<Push, Pull>(param).MapResult(
                (Push opt) => opt.Run(logger),
                (Pull opt) => opt.Run(logger),
             err => 1);
        }

    }

    public static class Extension {
        public static void WL<T>(this IEnumerable<T> @this) {
            foreach (var item in @this) {
                Console.WriteLine(item);
            }
        }

        public static void WL(this object @this) => Console.WriteLine(@this);
        public static void WL(this string @this) => Console.WriteLine(@this);
    }
}