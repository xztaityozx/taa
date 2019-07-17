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
            
            return true;
        }
    }
}