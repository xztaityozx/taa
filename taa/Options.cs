using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using MongoDB.Driver.Linq;

namespace taa {

    public interface IOption {
        int Run(Logger.Logger logger);

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
        
        [Option("parallel", Default = 10, HelpText = "並列実行数です")]
        int Parallel { get; set; }
    }
}