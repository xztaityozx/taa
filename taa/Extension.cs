using System;
using System.Collections.Generic;
using System.IO;

namespace taa {
    public static class Extension {
        public static void WL<T>(this IEnumerable<T> @this) {
            foreach (var item in @this) {
                Console.WriteLine(item);
            }
        }

        public static void WL(this object @this) => Console.WriteLine(@this);
        public static void WL(this string @this) => Console.WriteLine(@this);
    }

    public static class FilePath {
        public static string Expand(string path) {
            var split = path.Split(Path.DirectorySeparatorChar);
            if (split[0] != "~") return Path.GetFullPath(path);

            split[0] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(split);

            return Path.GetFullPath(path);
        }
    }
}