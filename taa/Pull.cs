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
        
        [Option("seed", Default = 1, HelpText = "Seed値")]
        public int Seed { get; set; }
        
        public override int Run() {
            
            return 0;
        }

    }
}