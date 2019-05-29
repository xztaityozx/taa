using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace taa {
    public class Record {
        public ObjectId Id { get; set; }

        [BsonElement("vtn")] public Transistor Vtn { get; set; }
        [BsonElement("vtp")] public Transistor Vtp { get; set; }
        [BsonElement("values")] public decimal[] Values { get; set; }
        [BsonElement("seed")] public int Seed { get; set; }
        [BsonElement("sweeps")] public int Sweeps { get; set; }
        [BsonElement("key")] public string Key { get; set; }

       
        public override string ToString() {
            return
                $" Signal/Time: {Key}, Seed: {Seed},, Sweeps: {Sweeps}, Vtn: {Vtn}, Vtp: {Vtp}, Values: [{string.Join(",", Values)}]";
        }

        public FilterDefinition<Record> UpdateFilter()
            => Builders<Record>.Filter.Where(s =>
                s.Key == Key &&
                s.Sweeps == Sweeps &&
                s.Vtn == Vtn &&
                s.Vtp == Vtp &&
                s.Seed == Seed
            );


    }
}