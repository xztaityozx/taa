using System;
using System.Globalization;

namespace taa.Parameter {
    public class Parameter {
        public Transistor Vtn { get; }
        public Transistor Vtp { get; }

        public Parameter(
            Transistor vtn, Transistor vtp
        ) {
            Vtn = vtn;
            Vtp = vtp;
        }

        public Parameter(
            double vtnThreshold, double vtnSigma, double vtnDeviation,
            double vtpThreshold, double vtpSigma, double vtpDeviation
        ) {
            Vtn = new Transistor(vtnThreshold, vtnSigma, vtnDeviation);
            Vtp = new Transistor(vtpThreshold, vtpSigma, vtpDeviation);
        }

        public static string EncodeKey(string signal, decimal time) => $"{signal}/{time:E10}";

        public static Tuple<string, decimal> DecodeKey(string key) {
            var split = key.Split("/");
            return Tuple.Create(split[0], decimal.Parse(split[1], NumberStyles.Float));
        }

        public override string ToString() {
            return $"vtn:{Vtn}_vtp:{Vtp}";
        }

        public string DatabaseName() =>
            $"vtn:v{Vtn.Voltage}_s{Vtn.Sigma}_d{Vtn.Deviation}vtp:v{Vtp.Voltage}_s{Vtp.Sigma}_d{Vtp.Deviation}";
    }
}