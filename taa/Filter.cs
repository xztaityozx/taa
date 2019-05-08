using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        [YamlMember(Alias = "targets")]
        public List<string> TargetList { get; }

        /// <summary>
        /// 答え
        /// </summary>
        [YamlMember(Alias = "answer")]
        public string Answer { get; }
    }

    public class Filter {
        private Config config;

        public Filter(Config config) => this.config = config;

        
    }
}