using System;
using System.Globalization;

namespace taa.Parameter {
    public class Parameter {
        public Transistor Vtn { get; }
        public Transistor Vtp { get; }
        public int Seed { get; }
        public string Key { get; }

        public Parameter(
            Transistor vtn, Transistor vtp,
            int seed, string signal, decimal time
        ) {
            Vtn = vtn;
            Vtp = vtp;
            Seed = seed;
            Key = EncodeKey(signal, time);
        }

        public static string EncodeKey(string signal, decimal time) => $"{signal}/{time:E10}";

        public static Tuple<string, decimal> DecodeKey(string key) {
            var split = key.Split("/");
            return Tuple.Create(split[0], decimal.Parse(split[1], NumberStyles.Float));
        }

        public override string ToString() {
            return $"vtn:{Vtn}_vtp:{Vtp}_seed:{Seed}";
        }
    }
}