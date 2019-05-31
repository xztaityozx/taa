using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace taa {
    public class Config {
        /// <summary>
        /// 評価式のリスト
        /// </summary>
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> Conditions { get; set; }

        /// <summary>
        /// 数え上げ対象の評価式のリスト
        /// </summary>
        [YamlMember(Alias = "expressions")]
        public List<string> Expressions { get; set; }

        /// <summary>
        /// 並列数
        /// </summary>
        [YamlMember(Alias = "parallel")]
        public int Parallel { get; set; }

        [YamlMember(Alias = "database")]
        public DatabaseConfig Database { get; set; }

        [YamlMember(Alias = "logDir")]
        public string LogDir { get; set; }

        public Config() { }

        public Config(string path, int parallel, string host, int port, string dbName) {
            string str;
            using (var sr = new StreamReader(path)) {
                str = sr.ReadToEnd();
            }

            var d = new Deserializer().Deserialize<Config>(str);

            Conditions = d.Conditions;
            Expressions = d.Expressions;
            Parallel = Set(parallel, d.Parallel, p => p != 0);
            Database = d.Database;
            Database.DataBaseName = Set(dbName, d.Database.DataBaseName, s => !string.IsNullOrEmpty(s));
            Database.Host = Set(host, d.Database.Host, s => !string.IsNullOrEmpty(s));
            Database.Port = Set(port, d.Database.Port, p => p >= 1024);

            LogDir = taa.FilePath.Expand(d.LogDir);
        }

        private static T Set<T>(T a, T b, Func<T,bool> func) {
            return func(a) ? a : b;
        }
    }

    public class DatabaseConfig {
        [YamlMember(Alias = "host")]
        public string Host { get; set; }
        [YamlMember(Alias = "port")]
        public int Port { get; set; }
        [YamlMember(Alias = "name")]
        public string DataBaseName { get; set; }

        public override string ToString() {
            return $"mongodb://{Host}:{Port}";
        }
    }
}