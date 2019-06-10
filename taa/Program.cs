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
            args = "get --end 100 --host 150.89.227.97 --port 28001 --dataBaseName test".Split(' ');
            var res = Parser.Default.ParseArguments<Push, Pull, Get>(args).MapResult(
                (Push p) => p.Run(),
                (Pull p) => p.Run(),
                (Get g) => g.Run(),
                err => false
            );

            if (!res) Environment.Exit(1);

        }
    }
}