using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;
using taa.Model;
using taa.Verb;

namespace taa.Extension {
    public static class Extension {

        public static void WL<T>(this IEnumerable<T> @this) {
            foreach (var item in @this) {
                Console.WriteLine(item);
            }
        }

        public static void WL(this object @this) => Console.WriteLine(@this);
        public static void WL(this string @this) => Console.WriteLine(@this);

        public static decimal ParseDecimalWithSiUnit(this string @this) {
            return decimal.Parse(@this.Replace("G", "E09")
                    .Replace("M", "E06")
                    .Replace("K", "E03")
                    .Replace("m", "E-03")
                    .Replace("u", "E-06")
                    .Replace("n", "E-09")
                    .Replace("p", "E-12"),
                NumberStyles.Float);
        }

        public static bool TryParseDecimalWithSiUnit(this string @this, out decimal value) {
            value = default;
            try {
                value = @this.ParseDecimalWithSiUnit();
                return true;
            }
            catch (FormatException ) {
                return false;
            }
        }

        /// <summary>
        /// @this ‚ª [start,end] ‚Ì”ÍˆÍ‚É‚ ‚é‚©‚Ç‚¤‚©‚ð•Ô‚·
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool Within<T>(this T @this, T start, T end) where T : IComparable<T> {
            return @this.CompareTo(start) >= 0 && end.CompareTo(@this) >= 0;
        }
    }

    public static class FilePath {
        public static string Expand(string path) {
            var split = path.Split('\\', '/');
            if (split[0] != "~") return Path.GetFullPath(path);

            split[0] = GetHome();
            path = Path.Combine(split);

            return Path.GetFullPath(path);
        }

        public static string GetHome() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}