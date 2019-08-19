using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Kurukuru;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using taa.Config;
using taa.Extension;
using taa.Factory;
using taa.Model;
using taa.Parameter;
using taa.Repository;

namespace taa.Verb {
    [Verb("get", HelpText = "数え上げます")]
    public class Get : SubCommand {

        [Option('w', "sweepRange", Default = "1,5000", HelpText = "Sweepの範囲を[開始],[終了値]で指定できます")]
        public string SweepRange { get; set; }

        [Option('e', "seedRange", Default = "1,2000", HelpText = "Seedの範囲を[開始],[終了値]で指定できます")]
        public string SeedRange { get; set; }

        [Option('i', "sigmaRange", Default = "", HelpText = "シグマの範囲を[開始],[刻み幅],[終了値]で指定できます")]
        public string SigmaRange { get; set; }

        [Option("out", Default = "./out.csv", HelpText = "結果を出力するCSVファイルへのパスです")]
        public string OutFile { get; set; }

        private static IEnumerable<Tuple<string, long>> Do(Transistor vtn, Transistor vtp, Filter filter, long sweepStart, long sweepEnd,
            long seedStart, long seedEnd) {
            var dn = Transistor.ToTableName(vtn, vtp);

            var repo = new MssqlRepository();
            repo.Use(dn);
            return repo.Count(r => r.Sweep.Within(sweepStart, sweepEnd) && r.Seed.Within(seedStart, seedEnd),
                filter);

        }

        public override Exception Run(CancellationToken token) {
            LoadConfig("~/.config/taa/config.yml");

            // SiUnitを考慮してオプションをパース
            var sigmaRange = SigmaRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (double) x.ParseDecimalWithSiUnit()).ToArray();
            var sweepRange = SweepRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiUnit()).ToArray();
            var seedRange = SeedRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiUnit()).ToArray();

            // 数え上げ用のFilterをBuild
            Logger.Info("Start build filter");
            var filter = new Filter(Config.Config.GetInstance().Conditions, Config.Config.GetInstance().Expressions);
            Logger.Info("Finished build filter");

            var result = new Map<string, Map<decimal, long>>();
            foreach (var s in filter.Delegates) {
                result[s.Name] = new Map<decimal, long>();
            }

            var sweepStart = sweepRange[0];
            var sweepEnd = sweepRange[1];
            var seedStart = seedRange[0];
            var seedEnd = seedRange[1];


            Logger.Info("Vtn:");
            Logger.Info($"\tVoltage: {VtnVoltage}");
            Logger.Info($"\tSigma: {VtnSigma}");
            Logger.Info($"\tDeviation: {VtnDeviation}");
            Logger.Info("Vtp:");
            Logger.Info($"\tVoltage: {VtpVoltage}");
            Logger.Info($"\tSigma: {VtpSigma}");
            Logger.Info($"\tDeviation: {VtpDeviation}");

            Logger.Info($"Sweeps: start: {sweepStart}, end: {sweepEnd}");
            Logger.Info($"Seed: start: {seedStart}, end: {seedEnd}");

            var sigmaList = new List<double>();

            if (sigmaRange.Length == 0) {
                Spinner.Start("Aggregating...", () => {

                    // Sigmaが固定
                    var vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation);
                    var vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation);

                    sigmaList.Add(VtnSigma);

                    var res = Do(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                    foreach (var (key, value) in res) {
                        result[key][vtn.Sigma] = value;
                    }
                });
            }
            else {
                // Sigmaを動かす
                var sigmaStart = sigmaRange[0];
                var sigmaStep = sigmaRange[1];
                var sigmaStop = sigmaRange[2];
                Logger.Info($"Range Sigma: start: {sigmaStart}, step: {sigmaStep}, stop: {sigmaStop}");

                for (var s = sigmaStart; s <= sigmaStop; s += sigmaStep) sigmaList.Add(s);

                using (var bar = new ProgressBar(sigmaList.Count, "Aggregating...", new ProgressBarOptions {
                    ForegroundColor = ConsoleColor.DarkBlue,
                    BackgroundCharacter = '-',
                    ForegroundColorDone = ConsoleColor.Green,
                    ProgressCharacter = '>',
                    BackgroundColor = ConsoleColor.DarkGray
                })) {
                    foreach (var sigma in sigmaList) {
                        var vtn = new Transistor(VtnVoltage, sigma, VtnDeviation);
                        var vtp = new Transistor(VtpVoltage, sigma, VtpDeviation);

                        var res = Do(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                        foreach (var (key, value) in res) {
                            result[key][vtn.Sigma] = value;
                        }

                        bar.Tick();
                    }
                }
            }

            // 出力
            Logger.Info("\n" + filter);
            var box = new List<IEnumerable<string>> {
                new[] {
                    "VtnThreshold",
                    "VtnSigma",
                    "VtnDeviation",
                    "VtpThreshold",
                    "VtpSigma",
                    "VtpDeviation",
                    "TotalSweeps"
                },
                new[] {
                    $"{VtnVoltage}",
                    $"{(sigmaRange.Length == 0 ? $"{VtnSigma}" : SigmaRange)}",
                    $"{VtnDeviation}",
                    $"{VtpVoltage}",
                    $"{(sigmaRange.Length == 0 ? $"{VtpSigma}" : SigmaRange)}",
                    $"{VtpDeviation}",
                    $"{(sweepEnd - sweepStart + 1) * (seedEnd - seedStart + 1)}"
                },
               new[] {
                    "Filter"
                }.Concat(sigmaList.Select(x => $"{x}"))
            };


            foreach (var (key, value) in result) {
                box.Add(new[] {key}.Concat(value.Select(x => $"{x.Value}")));
            }

            using (var sw = new StreamWriter(OutFile)) {
                foreach (var line in box) {
                    var item = string.Join(",", line);
                    sw.WriteLine(item);
                    item.WL();
                }
            }

            return null;
        }
    }
}