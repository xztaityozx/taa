using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Logger;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var res = Parser.Default.ParseArguments<Push, Pull>(args).MapResult(
                (Push p) => p.Run(),
                (Pull p) => p.Run(),
                err => false
            );

            if (!res) Environment.Exit(1);
        }
    }
}