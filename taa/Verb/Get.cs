using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Kurukuru;
using Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    [Verb("get", HelpText = "数え上げます")]
    public class Get : SubCommand {

        [Option("start", Default = 1, HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }

        [Option("end", Default = 2000, HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }

        [Option('x',"pullParallel", Default = 1, HelpText = "データベースからデータを取り出すパイプラインの並列数です")]
        public int PullParallel { get; set; }

        [Option('y', "buildParallel", Default = 1, HelpText = "取り出したデータをまとめるパイプラインの並列数です")]
        public int BuildParallel { get; set; }

        [Option('z', "aggregateParallel", Default = 1, HelpText="数え上げの並列数です")]
        public int AggregateParallel { get; set; }

        public override bool Run() {
            LoadConfig();
            return true;
        }
    }
}