
using Microsoft.EntityFrameworkCore;

namespace taa.Parameter {
    [Owned]
    public class Transistor {
        public static string ToTableName(Transistor vtn, Transistor vtp) => $"vtn_{vtn}_vtp_{vtp}";

        public bool Equals(Transistor other) {
            return Voltage == other.Voltage && Sigma == other.Sigma && Deviation == other.Deviation;
        }

        public override bool Equals(object obj) {
            return obj is Transistor other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Voltage.GetHashCode();
                hashCode = (hashCode * 397) ^ Sigma.GetHashCode();
                hashCode = (hashCode * 397) ^ Deviation.GetHashCode();
                return hashCode;
            }
        }

        public decimal Voltage { get; set; }
        public decimal Sigma { get; set; }
        public decimal Deviation { get; set; }

        public Transistor(double v, double s, double d) {
            Voltage = (decimal) v;
            Sigma = (decimal) s;
            Deviation = (decimal) d;
        }

        public override string ToString() {
            return $"v{Voltage}_s{Sigma}_d{Deviation}";
        }

        public static bool operator ==(Transistor s, Transistor t) {
            return s.Deviation == t.Deviation && s.Sigma == t.Sigma && s.Voltage == t.Voltage;
        }

        public static bool operator !=(Transistor s, Transistor t) {
            return !(s == t);
        }
    }
}