namespace taa.Parameter {
    public class Parameter {
        public Transistor Vtn { get; }
        public Transistor Vtp { get; }
        public int Seed { get; }

        public override string ToString() {
            return $"vtn:{Vtn}_vtp:{Vtp}_seed:{Seed}";
        }
    }
}