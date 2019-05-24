using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using MongoDB.Driver.Linq;

namespace taa {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : IOption {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option("seed", Default = 1, HelpText = "Seedの値です")]
        public int Seed { get; set; }

        private Config config;

        public int Run() {
            var d = new Document(InputFile, Sweeps, Seed);

            config = Config.Deserialize(ConfigFile);
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
            
            _ = repo.PushMany(records);

            return 0;
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }

        public string CollectionName { get; set; }
        public int Sweeps { get; set; }
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

    public interface IOption {
        int Run();

        [Option("config", Required = false, HelpText = "コンフィグファイルへのパスです")]
        string ConfigFile { get; set; }

        [Option('N', "VtnVoltage", Default = 0.6, Required = false,  HelpText = "Vtnの閾値電圧です")]
        double VtnVoltage { get; set; }

        [Option("vtnSigma", Default = 0.046, Required = false, HelpText = "Vtnのシグマです")]
        double VtnSigma { get; set; }

        [Option("vtnDeviation", Default = 1.0, Required = false,  HelpText = "Vtnの偏差です")]
        double VtnDeviation { get; set; }

        [Option('N', "VtpVoltage", Default = -0.6, Required = false,  HelpText = "Vtpの閾値電圧です")]
        double VtpVoltage { get; set; }

        [Option("vtnSigma", Required = false, Default = 0.046, HelpText = "Vtpのシグマです")]
        double VtpSigma { get; set; }

        [Option("vtnDeviation", Required = false, Default = 1.0, HelpText = "Vtpの偏差です")]
        double VtpDeviation { get; set; }

        [Option('s', "sigma", Required = false, Default  = 0.046, HelpText = "Vtn,Vtpのシグマです.個別設定が優先されます")]
        double Sigma { get; set; }

        [Option( "host", Required = false, Default = "localhost", HelpText = "データベースのホスト名です")]
        string Host { get; set; }

        [Option('p', "port", Required = false, Default = 27017, HelpText = "データベースサーバーのポートです")]
        int Port { get; set; }

        [Option("dataBaseName", Required = false, Default = "results", HelpText = "name of database")]
        string DatabaseName { get; set; }

        [Option('c', "collection",  Required = false, Default = "records", HelpText = "name of collection")]
        string CollectionName { get; set; }

        [Option("sweeps", Required = false, Default = 5000, HelpText = "number of sweeps")]
        int Sweeps { get; set; }
    }
}