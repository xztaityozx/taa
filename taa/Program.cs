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

            using (var cts = new CancellationTokenSource()) {
                Console.CancelKeyPress += (sender, eventArgs) => cts.Cancel();

                var token = cts.Token;

                var parser = Parser.Default;
                var res = parser.ParseArguments<Push, Get>(args).MapResult(
                    (Push p) => p.Run(token),
                    (Get g) => g.Run(token),
                    err => null
                );
                Console.ResetColor();
                if (res == null) return;
                res.WL();
                Environment.Exit(1);
            }
        }

    }
}
