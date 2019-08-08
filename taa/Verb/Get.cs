using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommandLine;
using Kurukuru;
using Microsoft.EntityFrameworkCore;
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

        private Tuple<string, long>[] Do(Transistor vtn, Transistor vtp, Filter filter, long sweepStart, long sweepEnd,
            long seedStart, long seedEnd) {
            var dn = Transistor.ToTableName(vtn, vtp);

            using (var repo = new MssqlRepository(dn,
                mb => mb.Entity<RecordModel>().HasKey(e => new { e.Sweep, e.Key, e.Seed }))) {
                return repo.Count(r => r.Sweep.Within(sweepStart, sweepEnd) && r.Seed.Within(seedStart, seedEnd),
                    filter);
            }
        }

        public override Exception Run() {
            LoadConfig("~/.config/taa/config.yml");

            var sigmaRange = SigmaRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (double) x.ParseDecimalWithSiUnit()).ToArray();
            var sweepRange = SweepRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiUnit()).ToArray();
            var seedRange = SeedRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiUnit()).ToArray();

            var filter = new Filter(Config.Config.GetInstance().Conditions, Config.Config.GetInstance().Expressions);

            var map = new Map<string, Map<decimal, long>>();
            foreach (var s in filter.Delegates) {
                map[s.Name] = new Map<decimal, long>();
            }

            var sweepStart = sweepRange[0];
            var sweepEnd = sweepRange[1];
            var seedStart = seedRange[0];
            var seedEnd = seedRange[1];

            var sigmaList = new List<double>();

            Spinner.Start("Aggregating...", () => {
                if (sigmaRange.Length == 0) {
                    // Sigmaが固定
                    var vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation);
                    var vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation);

                    sigmaList.Add(VtnSigma);

                    var res = Do(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                    foreach (var (key, value) in res) {
                        map[key][vtn.Sigma] = value;
                    }
                }
                else {
                    // Sigmaを動かす
                    for (var sigma = sigmaRange[0]; sigma <= sigmaRange[2]; sigma += sigmaRange[1]) {
                        var vtn = new Transistor(VtnVoltage, sigma, VtnDeviation);
                        var vtp = new Transistor(VtpVoltage, sigma, VtpDeviation);
                        sigmaList.Add(sigma);

                        var res = Do(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                        foreach (var (key, value) in res) {
                            map[key][vtn.Sigma] = value;
                        }
                    }
                }
            });

            var box = new List<IEnumerable<string>> {
                new[] {
                    $"VtnThreshold: {VtnVoltage}",
                    $"VtnSigma: {(sigmaRange.Length == 0 ? $"{VtnSigma}" : SigmaRange)}",
                    $"VtnDeviation: {VtnDeviation}",
                    $"VtpThreshold: {VtpVoltage}",
                    $"VtpSigma: {(sigmaRange.Length == 0 ? $"{VtpSigma}" : SigmaRange)}",
                    $"VtpDeviation: {VtpDeviation}",
                    $"TotalSweeps: {(sweepEnd - sweepStart + 1) * (seedEnd - seedStart + 1)}"
                },
                new[] {
                    "Filter"
                }.Concat(sigmaList.Select(x => $"{x}"))
            };


            foreach (var (key, value) in map) {
                box.Add(new[] {key}.Concat(value.Select(x => $"{x}")));
            }

            Spinner.Start($"Write to {OutFile}", () => {
                using (var sw = new StreamWriter(OutFile)) {
                    foreach (var line in box) {
                        var item = string.Join(",", line);
                        sw.WriteLine(item);
                        item.WL();
                    }
                }
            });
            
            return null;
        }
    }
}