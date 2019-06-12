using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Logger;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var res = Parser.Default.ParseArguments<Push, Pull, Get>(args).MapResult(
                (Push p) => p.Run(),
                (Pull p) => p.Run(),
                (Get g) => g.Run(),
                err => false
            );

            Console.ResetColor();
            Console.WriteLine("Taa Finished");

            if (!res) Environment.Exit(1);

        }
    }
}