using System;
using System.Collections.Generic;
using CommandLine;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : IOption {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option("seed", Default = 1, HelpText = "Seedの値です")]
        public int Seed { get; set; }

        private Config config;

        public int Run(Logger.Logger logger) {
            logger.Info("Start push");
            
            var d = new Document(InputFile, Sweeps, Seed);

            config = !string.IsNullOrEmpty(ConfigFile) ? Config.Deserialize(ConfigFile) : new Config(Parallel,Sweeps);
            config.SetOrDefault(Host, Port, DatabaseName, CollectionName);

            // 
            if (VtnSigma == 0.046) VtnSigma = Sigma;
            if (VtpSigma == 0.046) VtpSigma = Sigma;

            var vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation);
            var vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation);

            var records = new List<Record>();

            var table = new Map<string, List<decimal>>();

            foreach (var map in d) {
                foreach (var (key, value) in map) {
                    if (table[key] == null) table[key] = new List<decimal>();

                    table[key].Add(value);
                }
            }

            foreach (var (key, value) in table) {
                var (signal, time) = Document.DecodeKey(key);
                records.Add(new Record(vtn, vtp, time, value, signal, d.Seed));
            }
            
            var repo = new Repository(config.DatabaseConfig);
            logger.Info(config.DatabaseConfig.ToString());
            
            _ = repo.PushMany(records);

            logger.Info("Finished push");
            return 0;
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }

        public string CollectionName { get; set; }
        public int Sweeps { get; set; }
        public int Parallel { get; set; }
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
    }
}