using System;
using System.Collections.Generic;
using System.IO;
using taa.Extension;
using YamlDotNet.Serialization;

namespace taa.Config {
    public sealed class Config {
        // conditions BNF
        // <condition> := <name>: <cond>
        // <cond> := <value><operator><value>
        // <value> := <signal>, <number>
        // <signal> := <signalName>[<time>]
        // <time> := <number>
        // <number> := [0-9](.[0-9]+)*, [0-9](.[0-9]+)<siUnit> 
        // <siUnit> := G, M, K, m, u, n, p
        // <operator> := <, <=, >, >=, !=, ==
        // <signalName> := <string>
        // <name> := <string>
        // <string> := ([a-zA-Z0-9])+
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> Conditions { get; set; }

        [YamlMember(Alias = "expressions")]
        public List<string> Expressions { get; set; }
        [YamlMember(Alias = "logDir")]
        public string LogDir { get; set; }
        [YamlMember(Alias = "connection")]
        public string ConnectionsString { get; set; }
        [YamlMember(Alias = "machine")]
        public string MachineName { get; set; }

        public Config() { }

        private static Config instance;

        public static Config GetInstance(string path = "") {
                if (instance != null) return instance;
                if (string.IsNullOrEmpty(path)) throw new NullReferenceException("パスが設定されていません");

                path = FilePath.Expand(path);

                string str;
                try {
                    using (var sr = new StreamReader(path)) str = sr.ReadToEnd();
                }
                catch (FileNotFoundException) {
                    throw new FileNotFoundException($"ファイルが見つかりません: {path}");
                }

                try {
                    instance = new Deserializer().Deserialize<Config>(str);
                }
                catch (Exception) {
                    Console.Error.WriteLine("パースできませんでした");
                    throw;
                }

                instance.LogDir = FilePath.Expand(instance.LogDir);

                return instance;
        }
    }
}