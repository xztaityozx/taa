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
            Config.Config.GetInstance("~/.config/taa/config.yml");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => cts.Cancel();

            var param = @"push -b 100000 -d C:\Users\xztaityozx\source\repos\xztaityozx\taa\UnitTest\file\".Split(' ',
                StringSplitOptions.RemoveEmptyEntries);

            var parser = Parser.Default;
            var res = parser.ParseArguments<Push, Get>(param).MapResult(
                (Push p) => p.Run(cts.Token),
                (Get g) => g.Run(cts.Token),
                err => null
            );

            Console.ResetColor();
            if (res == null) return;
            res.WL();
            Environment.Exit(1);

        }

    }
}
