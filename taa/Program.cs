using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Logger;
using ShellProgressBar;
using taa.Extension;
using taa.Factory;
using taa.Verb;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {

            //var param = @"push --seed 6 C:\Users\xztaityozx\source\repos\xztaityozx\taa\UnitTest\file\SEED00006.csv".Split(" ");
            var param = @"get --start 6 --end 6".Split(" ");

            Config.Config.GetInstance("~/.config/taa/config.yml");

            var parser = Parser.Default;

            var res = parser.ParseArguments<Push, Get>(param).MapResult(
                (Push p) => p.Run(),
                (Get g) => g.Run(),
                err => false
            );

            Console.ResetColor();
            if (!res) Environment.Exit(1);
        }

    }
}
