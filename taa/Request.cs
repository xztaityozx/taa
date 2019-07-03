using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace taa {
    public class Request {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public int Sweeps { get; set; }
        public int SeedStart { get; set; }
        public int SeedEnd { get; set; }
        public List<string> Keys { get; set; }


        public override string ToString() {
            return $@"Vtn: {Vtn}
Vtp: {Vtp}
Sweeps: {Sweeps}
Seed:
    Start: {SeedStart}
    End:   {SeedEnd}";
        }
    }
}