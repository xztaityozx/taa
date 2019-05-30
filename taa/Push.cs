using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Kurukuru;
using MongoDB.Driver;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option("seed", Default = 1, HelpText = "Seedの値です")]
        public int Seed { get; set; }

        public override bool RunSuppress() {
            
        }

        public override bool Run() {
            LoadConfig();
            

            Logger.Info("Finished Push");
            return true;
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }

    }
}