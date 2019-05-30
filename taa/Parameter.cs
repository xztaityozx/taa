using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace taa {
    public class Parameter {
        public ObjectId Id { get; set; }
        [BsonElement("vtn")]
        public Transistor Vtn { get; set; }
        [BsonElement("vtp")]
        public Transistor Vtp { get; set; }
        [BsonElement("sweeps")]
        public int Sweeps { get; set; }
    }
}