using System;
using System.Globalization;

namespace taa.Document {
    public class Document {
        public static decimal ParseDecimalWithSiUnit(string s) {
            return decimal.Parse(s.Replace("G", "E09")
                    .Replace("M", "E06")
                    .Replace("K", "E03")
                    .Replace("m", "E-03")
                    .Replace("u", "E-06")
                    .Replace("n", "E-09")
                    .Replace("p", "E-12"),
                NumberStyles.Float);
        }
        public static bool TryParseDecimalWithSiUnit(string s, out decimal v) {
            v = default;
            try {
                v = ParseDecimalWithSiUnit(s);
            }
            catch (FormatException) {
                return false;
            }
            return true;
        }
    }
}