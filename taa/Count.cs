using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace taa {
    [Verb("count", HelpText = "カウントします")]
    public class Count : SubCommand {
        public override bool Run() {
            return true;
        }

    }
}