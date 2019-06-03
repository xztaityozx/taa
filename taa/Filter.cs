using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DynamicExpresso;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace taa {

    public class Filter {
        private readonly Config config;
        public Filter(Config config) {
            this.config = config;
        }

        public void Build() {
            var keys = new List<string>();
            
            foreach (var condition in config.Conditions) {
            }
        }
    }
}