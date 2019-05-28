using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace taa {
    [Verb("count", HelpText = "カウントします")]
    public class Count : IOption {
        
        [Option("start",Default = 1,HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }
        
        [Option("end",Default = 2000,HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }
        
        public int Run(Logger.Logger logger) {
            logger.Info("Start count");

            ConfigFile = string.IsNullOrEmpty(ConfigFile)
                ? Path.Combine(
                    Environment.GetEnvironmentVariable("HOME"),
                    ".config",
                    "taa",
                    "config.yml"
                )
                : ConfigFile;

            
            var dispatcher = new Dispatcher(ConfigFile);

            logger.Info("Finished count");
            return 0;
        }

        public string ConfigFile { get; set; } 
        public double VtnVoltage { get; set; }
        public double VtnSigma { get; set; }
        public double VtnDeviation { get; set; }
        public double VtpVoltage { get; set; }
        public double VtpSigma { get; set; }
        public double VtpDeviation { get; set; }
        public double Sigma { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public int Sweeps { get; set; }
        public int Parallel { get; set; }
    }
}