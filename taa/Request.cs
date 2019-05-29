using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace taa {
    public class Request {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public int Sweeps { get; set; }
        public int SeedStart { get; set; }
        public int SeedEnd { get; set; }
        public string Key { get; set; }

        public static IEnumerable<Request> GenerateRequests(
            Transistor vtn, Transistor vtp, 
            int sweeps, int ss, int se,
            IEnumerable<string> key) {
            return key.Select(k => new Request {
                    Vtn = vtn,
                    Vtp = vtp,
                    Sweeps = sweeps,
                    SeedStart = ss,
                    SeedEnd = se,
                    Key = k
                })
                .ToList();
        }

        public FilterDefinition<Record> FindFiler() {
            return Builders<Record>.Filter.Where(r =>
                Vtn == r.Vtn && Vtp == r.Vtp &&
                Sweeps == r.Sweeps && Key == r.Key &&
                r.Seed >= SeedStart && r.Seed <= SeedEnd
            );
        }
    }
}