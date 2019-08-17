using System;
using System.IO;
using System.Threading;
using CommandLine;
using Logger;

namespace taa.Verb {
    public abstract class SubCommand : ISubCommand {
        protected Logger.Logger Logger = new Logger.Logger();
        public abstract Exception Run(CancellationToken token);

        public void LoadConfig(string path) {
            Config.Config.GetInstance(string.IsNullOrEmpty(ConfigFile) ? path : ConfigFile);
            var now = DateTime.Now;
            Logger.AddHook(new FileHook(
                Path.Combine(
                    Config.Config.GetInstance().LogDir,
                    $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2}-{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}-{now.Millisecond:D2}.log"
                )
            ));
            Bind();
        }

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