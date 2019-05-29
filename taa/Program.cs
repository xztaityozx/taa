using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Logger;
using MongoDB.Driver;

namespace taa {
    internal class Program {
        private static void Main(string[] args) {
        }

    }

    public static class Extension {
        public static void WL<T>(this IEnumerable<T> @this) {
            foreach (var item in @this) {
                Console.WriteLine(item);
            }
        }

        public static void WL(this object @this) => Console.WriteLine(@this);
        public static void WL(this string @this) => Console.WriteLine(@this);
    }
}