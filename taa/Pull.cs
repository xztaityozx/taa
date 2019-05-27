using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MongoDB.Driver.Linq;

namespace taa {
    [Verb("pull", HelpText = "DBからデータを取り出します")]
    public class Pull :IOption {
        private Config config;
        
        [Value(0,Required = true, HelpText = "信号名[時間]で取り出す値を指定します")]
        public IEnumerable<string> Request { get; set; }
        
        public int Run(Logger.Logger logger) {
            logger.Info("Start pull");
            
            config = !string.IsNullOrEmpty(ConfigFile) ? Config.Deserialize(ConfigFile) : new Config(Parallel,Sweeps);
            config.SetOrDefault(Host, Port, DatabaseName, CollectionName);

            if (VtnSigma == 0.046) VtnSigma = Sigma;
            if (VtpSigma == 0.046) VtpSigma = Sigma;
            
            var repo = new Repository(config.DatabaseConfig);

            var vtn = new Transistor(VtnVoltage, VtnSigma, VtnDeviation);
            var vtp = new Transistor(VtpVoltage, VtpSigma, VtpDeviation);

            var response = repo.Pull(vtn, vtp, Request.Select(Decode), Sweeps);

            foreach (var record in response) {
                Console.WriteLine(record.Time == Document.ParseDecimalWithSiUnit("2.5n"));
            }


            logger.Info("Finished pull");
            return 0;
        }

        private static Tuple<string, decimal> Decode(string key) {
            var split = key.Split('[', ']');
            var signal = split[0];
            var time = Document.ParseDecimalWithSiUnit(split[1]);

            return Tuple.Create(signal, time);
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