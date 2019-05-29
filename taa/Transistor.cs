using MongoDB.Bson.Serialization.Attributes;

namespace taa {
    public struct Transistor {
        public bool Equals(Transistor other) {
            return Voltage == other.Voltage && Sigma == other.Sigma && Deviation == other.Deviation;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
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

        [BsonElement("voltage")]
        public decimal Voltage { get; set; }
        [BsonElement("sigma")]
        public decimal Sigma { get; set; }
        [BsonElement("deviation")]
        public decimal Deviation { get; set; }

        public Transistor(double v, double s, double d) {
            Voltage = (decimal) v;
            Sigma = (decimal) s;
            Deviation = (decimal) d;
        }
        
        public override string ToString() {
            return $"[ Voltage: {Voltage}, Sigma: {Sigma}, Deviation: {Deviation} ]";
        }

        public static  bool operator ==(Transistor s, Transistor t) {
            return s.Deviation == t.Deviation && s.Sigma == t.Sigma && s.Voltage == t.Voltage;
        }

        public static bool operator !=(Transistor s, Transistor t) {
            return !(s == t);
        }
    }
}