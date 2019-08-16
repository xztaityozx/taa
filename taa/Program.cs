using System;
using System.Collections.Generic;
using System.IO;
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

            //var param = @"push C:\Users\xztaityozx\source\repos\xztaityozx\taa\UnitTest\file\00006".Split(" ", StringSplitOptions.RemoveEmptyEntries);
            //var param = @"get --start 6 --end 6".Split(" ");

            Config.Config.GetInstance("~/.config/taa/config.yml");

            var parser = Parser.Default;

            var res = parser.ParseArguments<Push, Get>(args).MapResult(
                (Push p) => p.Run(),
                (Get g) => g.Run(),
                err => null
            );

            Console.ResetColor();
            if (res == null) return;
            res.WL();
            Environment.Exit(1);
        }

    }
}
