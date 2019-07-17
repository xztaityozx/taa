using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option('S',"seed",HelpText = "シード値です", Default = 1)]
        public int Seed { get; set; }

        public override bool Run() {
            LoadConfig();

            return true;
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }
    }
}