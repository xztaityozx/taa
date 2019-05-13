﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using NLua;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
            var dir = @"/home/xztaityozx/TestDir/";

            var r = WvCsvParser.Parse(dir, new[] {"N1","N2"}, 5, 5000);
            
            Console.WriteLine(r);

        }
    }
}