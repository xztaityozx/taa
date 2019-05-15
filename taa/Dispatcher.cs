using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;
using YamlDotNet.Serialization;

namespace taa {
    public class Dispatcher {
        private readonly Config config;

        public Dispatcher(string configFile) {
            string str;
            using (var sr = new StreamReader(configFile)) {
                str = sr.ReadToEnd();
            }

            var d = new Deserializer();
            config = d.Deserialize<Config>(str);
        }

        public Dispatcher(Config config) => this.config = config;

        private readonly ProgressBarOptions option= new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Blue,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '█',
            BackgroundCharacter = '▒'
        };

        private readonly ProgressBarOptions childOption = new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '>',
        };

        private readonly ProgressBarOptions innerOption = new ProgressBarOptions {
            ForegroundColor = ConsoleColor.Magenta,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '-',
            BackgroundCharacter = ' '
        };



    }
}