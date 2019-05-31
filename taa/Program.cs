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
            const string path = "/home/xztaityozx/TestDir/m8d/SEED00001.csv";
            var param =
                ("push --host 150.89.227.97 --port 28001 -N 1 -P 4 --vtnSigma 2 --vtpSigma 5 --vtnDeviation 3 --vtpDeviation 6 " + path).Split(' ');
            
            var res = Parser.Default.ParseArguments<Push, Pull>(param).MapResult(
                (Push p) => p.Run(),
                (Pull p) => p.Run(),
                err => false
            );

            if (!res) Environment.Exit(1);

        }

    }

}