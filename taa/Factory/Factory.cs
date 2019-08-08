using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace taa.Factory {
    public static class Factory {
        public static string Hash<T>(params T[] items) {
            return SHA256.Create().ComputeHash(
                Encoding.Unicode.GetBytes(
                    items.Aggregate("", (s, t) => $"{s}{t}")
                )
            ).Aggregate("", (s, b) => $"{s}{b:X}");
        }
    }
}
