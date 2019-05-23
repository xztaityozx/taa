using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace taa {
    public class Config {
        /// <summary>
        /// 評価式のリスト
        /// </summary>
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> ConditionList { get; }

        /// <summary>
        /// 数え上げ対象の評価式のリスト
        /// </summary>
        [YamlMember(Alias = "expressions")]
        public List<string> ExpressionList { get; }

        /// <summary>
        /// 並列数
        /// </summary>
        [YamlMember(Alias = "parallel")]
        public int Parallel { get; }

        [YamlMember(Alias = "times")] public int Times { get; }

        [YamlMember(Alias = "sweeps")] public int Sweep { get; }

        [YamlMember(Alias = "database")] public DatabaseConfig DatabaseConfig { get; }

        public Config(int p, int time, int sweep) {
            ConditionList = new Dictionary<string, string>();
            ExpressionList = new List<string>();
            Parallel = p;
            Times = time;
            Sweep = sweep;
        }

        public void AddCondition(string key, string value) => ConditionList.Add(key, value);
        public void AddExpression(string target) => ExpressionList.Add(target);

        public Config Deserialize(string path) {
            string str;
            using (var sr = new StreamReader(path)) {
                str = sr.ReadToEnd();
            }

            var d = new Deserializer();
            return d.Deserialize<Config>(str);
        }
    }

    public class DatabaseConfig {
        [YamlMember]
        public string Host { get; }
        [YamlMember]
        public int Port { get; }
        [YamlMember]
        public string Name { get; }
        [YamlMember]
        public string Collection { get; }

        public override string ToString() {
            return $"mongodb://{Host}:{Port}";
        }

        public DatabaseConfig(string host, int port, string name, string collection) {
            Host = host;
            Port = port;
            Name = name;
            Collection = collection;
        }
    }
}