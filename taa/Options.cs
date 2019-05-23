using System;
using System.IO;
using CommandLine;

namespace taa {

    [Verb("push", HelpText = "DBにデータをPushします")]
    public class PushOption :IOption {

        [Value(0, HelpText = "入力ファイルです",MetaName = "input")]
        public string InputFile { get; }

        [Option("seed", Default = 1, HelpText = "Seedの値です")]
        public int Seed { get; set; }

        public int Run() {            
            return 0;
        }

        public string ConfigFile { get; set; }
        public decimal VtnVoltage { get; set; }
        public decimal VtnSigma { get; set; }
        public decimal VtnDeviation { get; set; }
        public decimal VtpVoltage { get; set; }
        public decimal VtpSigma { get; set; }
        public decimal VtpDeviation { get; set; }
        public decimal Sigma { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public interface IOption {
        int Run();
        [Option("config", Required = false, HelpText = "コンフィグファイルへのパスです")]
        string ConfigFile { get; set; }

        [Option('N', "VtnVoltage", Default = 0.6, HelpText = "Vtnの閾値電圧です")]
        decimal VtnVoltage { get; set; }

        [Option("vtnSigma", Default = 0.046, HelpText = "Vtnのシグマです")]
        decimal VtnSigma { get; set; }

        [Option("vtnDeviation", Default = 1.0, HelpText = "Vtnの偏差です")]
        decimal VtnDeviation { get; set; }

        [Option('N', "VtpVoltage", Default = 0.6, HelpText = "Vtpの閾値電圧です")]
        decimal VtpVoltage { get; set; }

        [Option("vtnSigma", Default = 0.046, HelpText = "Vtpのシグマです")]
        decimal VtpSigma { get; set; }

        [Option("vtnDeviation", Default = 1.0, HelpText = "Vtpの偏差です")]
        decimal VtpDeviation { get; set; }

        [Option('s',"sigma", Default = 0.046, HelpText = "Vtn,Vtpのシグマです.個別設定が優先されます")]
        decimal Sigma { get; set; }

        [Option('h', "host", HelpText = "データベースのホスト名です")]
        string Host { get; set; }
        [Option('p', "port", HelpText = "データベースサーバーのポートです")]
        int Port { get; set; }
    }
}