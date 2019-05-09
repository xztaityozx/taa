using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
            var str = "not false";

            var dt = new DataTable();

            Console.WriteLine((bool)dt.Compute(str,""));
        }

    }
}