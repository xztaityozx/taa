using System;
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
            var dir = @"C:\Users\xztaityozx\source\repos\taa\taa\test\m8d";

            var r = WvCsvParser.Parse(dir, new[] {"N1","N2"}, 9, 5000);

            Console.WriteLine(r);
        }
    }
}