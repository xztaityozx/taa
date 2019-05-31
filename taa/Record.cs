using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace taa {
    public class Record {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }
        [BsonElement("parameterId")]
        public ObjectId ParameterId { get; set; }

        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        [BsonElement("values")] public decimal[] Values { get; set; }
        [BsonElement("seed")] public int Seed { get; set; }
        public int Sweeps { get; set; }
        [BsonElement("key")] public string Key { get; set; }

       
        public override string ToString() {
            return
                $" Signal/Time: {Key}, Seed: {Seed}";
        }

        public FilterDefinition<Record> UpdateFindFilter()
            => Builders<Record>.Filter.Where(r => 
                r.ParameterId == ParameterId && r.Seed == Seed && r.Key == Key);
    }
}