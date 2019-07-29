using System;
using System.IO;
using CommandLine;
using Logger;

namespace taa.Verb {
    public abstract class SubCommand : ISubCommand {
        protected Logger.Logger Logger;
        public abstract bool Run();

        protected void Bind() {
            if ($"{Sigma}" != $"{-0.1}") VtnSigma = VtpSigma = Sigma;
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
        public int Sweeps { get; set; }
    }
    public interface ISubCommand {
        [Option("config", Required = false, HelpText = "コンフィグファイルへのパスです")]
        string ConfigFile { get; set; } 

        [Option('N', "VtnVoltage", Default = 0.6, Required = false,  HelpText = "Vtnの閾値電圧です")]
        double VtnVoltage { get; set; }

        [Option("vtnSigma", Default = 0.046, Required = false, HelpText = "Vtnのシグマです")]
        double VtnSigma { get; set; }

        [Option("vtnDeviation", Default = 1.0, Required = false,  HelpText = "Vtnの偏差です")]
        double VtnDeviation { get; set; }

        [Option('P', "VtpVoltage", Default = -0.6, Required = false,  HelpText = "Vtpの閾値電圧です")]
        double VtpVoltage { get; set; }

        [Option("vtpSigma", Required = false, Default = 0.046, HelpText = "Vtpのシグマです")]
        double VtpSigma { get; set; }

        [Option("vtpDeviation", Required = false, Default = 1.0, HelpText = "Vtpの偏差です")]
        double VtpDeviation { get; set; }

        [Option('s', "sigma", Required = false, Default  = -1.0, HelpText = "Vtn,Vtpのシグマです.個別設定が優先されます")]
        double Sigma { get; set; }

        [Option("sweeps", Required = false, Default = 5000, HelpText = "number of sweeps")]
        int Sweeps { get; set; }
    }
}