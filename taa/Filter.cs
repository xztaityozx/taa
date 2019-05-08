using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YamlDotNet.Serialization;

namespace taa {
    public class Filter {
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


        public Filter(Dictionary<string, string> d, List<string> t, string ans) {
            ConditionList = d;
            TargetList = t;
            Answer = ans;
            
        }

        
    }

}