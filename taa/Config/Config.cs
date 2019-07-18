using System;
using System.Collections.Generic;
using System.IO;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using taa.Extension;
using YamlDotNet.Serialization;

namespace taa.Config {
    public sealed class Config {
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> Conditions { get; set; }
        [YamlMember(Alias = "expressions")]
        public List<string> Expressions { get; set; }
        [YamlMember(Alias = "logDir")]
        public string LogDir { get; set; }
        [YamlMember(Alias = "connection")]
        public string ConnectionsString { get; set; }

        public Config() { }

        private static Config instance;

        public static Config GetInstance(string path = "") {
                if (instance != null) return instance;
                if (string.IsNullOrEmpty(path)) throw new NullReferenceException("コンフィグファイルへのパスが未設定です");

                path = FilePath.Expand(path);

                string str;
                try {
                    using (var sr = new StreamReader(path)) str = sr.ReadToEnd();
                }
                catch (FileNotFoundException) {
                    throw new FileNotFoundException($"コンフィグファイルが見つかりません: {path}");
                }

                try {
                    instance = new Deserializer().Deserialize<Config>(str);
                }
                catch (Exception) {
                    Console.Error.WriteLine("コンフィグファイルのパースに失敗しました");
                    throw;
                }

                instance.LogDir = FilePath.Expand(instance.LogDir);

                return instance;
        }
    }
}