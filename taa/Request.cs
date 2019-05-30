using System;
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
        public List<string> Key { get; set; }

        public FilterDefinition<Record>[] FindFilterDefinitions(ObjectId id)
            => Key.Select(key=> Builders<Record>.Filter.Where(r => 
                r.Seed <= SeedEnd && r.Seed >= SeedStart &&
                r.ParameterId == id &&
                r.Key == key)).ToArray();
                
                )
    }
}