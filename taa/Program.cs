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
            var param = "get --end 1 --host 150.89.227.97 --port 28001".Split(' ');
            var res = Parser.Default.ParseArguments<Push, Pull, Get>(param).MapResult(
                (Push p) => p.Run(),
                (Pull p) => p.Run(),
                (Get g) => g.Run(),
                err => false
            );

            if (!res) Environment.Exit(1);
        }
    }
}