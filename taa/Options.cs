using System;
using System.IO;
using CommandLine;

namespace taa {

    [Verb("push", HelpText = "DBにデータをPushします")]
    public class PushOption :IOption {
        [Option('h', "host", HelpText = "データベースのホスト名です")]
        public string Host { get; set; }
        [Option('p', "port", HelpText = "データベースサーバーのポートです")]
        public int Port { get; set; }

        [Value(0, HelpText = "入力ファイルです",MetaName = "input")]
        public string InputFile { get; }

        public int Run() {
            
            return 0;
        }

        public string ConfigFile { get; set; } 
    }

    public interface IOption {
        int Run();
        [Option("config", Required = false, HelpText = "コンフィグファイルへのパスです")]
        string ConfigFile { get; set; }
    }
}