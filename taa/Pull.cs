using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MongoDB.Driver.Linq;
using ShellProgressBar;

namespace taa {
    [Verb("pull", HelpText = "DBからデータを取り出します")]
    public class Pull :SubCommand {
        
        [Value(0,Required = true, HelpText = "信号名[時間]で取り出す値を指定します")]
        public IEnumerable<string> Request { get; set; }
        
        [Option("start", Default = 1, HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }

        [Option("end", Default = 2000, HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }
        
        public override bool Run() {
            LoadConfig();
            var request = new Request {
                Keys = Request.Select(r => {
                    var split = r.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
                    return Document.EncodeKey(split[0], Document.ParseDecimalWithSiUnit(split[1]));
                }).ToList(),
                Sweeps = Sweeps,
                Vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation),
                Vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation),
                SeedEnd = SeedEnd,
                SeedStart = SeedStart
            };

            var repo = new Repository(Config.Database);
            IEnumerable<Record> response = repo.Find(request.Keys, SeedStart, repo.FindParameter(request).Id);

            response.WL();

            return true;
        }
    }
}