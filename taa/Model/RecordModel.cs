using System;

namespace taa.Model {
    public class RecordModel {
        public long Id { get;  set; }
        public long Sweep { get;  set; }
        public decimal Value { get;  set; }
        public int Seed { get;  set; }
        public string Signal { get;  set; }
        public decimal Time { get;  set; }
        public string Key => EncodeKey(Signal, Time);

        public override string ToString() {
            return $"Signal:{Signal}, Time:{Time}, Sweep:{Sweep}, Value:{Value}, Seed:{Seed}";
        }

        public static string EncodeKey(string signal, decimal time) => $"{signal}/{time:E10}";
    }

}
