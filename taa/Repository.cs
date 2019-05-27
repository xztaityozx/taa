using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace taa {
    public class Repository  {
        private readonly DatabaseConfig config;
        private readonly IMongoDatabase db;

        public Repository(DatabaseConfig config) {

            this.config = config;
            var client = new MongoClient(config.ToString());
            db = client.GetDatabase(config.Name);

            if (db.ListCollectionNames().ToList().All(c => c != config.Collection)) {
                db.CreateCollection(config.Collection);
            }
        }

        public BulkWriteResult<Record> PushMany(IEnumerable<Record> records) {
            var collection = db.GetCollection<Record>(config.Collection);

            var requests = records.Select(record =>
                new UpdateOneModel<Record>(record.Filter, record.Update) {
                    IsUpsert = true
                }).ToList();
            return collection.BulkWrite(requests);
        }
        
        public Record[] Pull(Transistor vtn, Transistor vtp, IEnumerable<Tuple<string, decimal>> targets, int sweeps) {
            var col = db.GetCollection<Record>(config.Collection);
            //var tasks = new List<Task<IAsyncCursor<Record>>>();
            //foreach (var (signal, time) in targets) {
            //    tasks.Add(col.FindAsync(Record.FindFilter(vtn, vtp, signal, time, sweeps)));
            //}

            //var records = Task.WhenAll(tasks).Result.SelectMany(c => c.ToList());

            // TODO: r.Timeとtimeの比較がうまくできてない。意味不明マジで
            var records = new List<Record>();

            foreach (var (signal, time) in targets) {
                records.AddRange(col.Find(r=>
                    r.VtnDeviation==vtn.Deviation
                    ).ToList());
            }

            return records.ToArray();
        }
    }

    public class Transistor {
        public decimal Voltage { get; set; }
        public decimal Sigma { get; set; }
        public decimal Deviation { get; set; }

        public Transistor(double v, double s, double d) {
            Voltage = (decimal)v;
            Sigma = (decimal)s;
            Deviation = (decimal) d;
        }

    }

    public class Record {
        public ObjectId Id { get; set; }

        [BsonElement("vtn-voltage")]
        public decimal VtnVoltage { get; set; }
        [BsonElement("vtn-sigma")]
        public decimal VtnSigma { get; set; }
        [BsonElement("vtn-deviation")]
        public decimal VtnDeviation { get; set; }
        [BsonElement("vtp-voltage")]
        public decimal VtpVoltage { get; set; }
        [BsonElement("vtp-sigma")]
        public decimal VtpSigma { get; set; }
        [BsonElement("vtp-deviation")]
        public decimal VtpDeviation { get; set; }
        [BsonElement("time")]
        public decimal Time { get; set; }
        [BsonElement("values")]
        public decimal[] Values { get; set; }
        [BsonElement("signal")]
        public string Signal { get; set; }
        [BsonElement("seed")]
        public int Seed { get; set; }
        [BsonElement("sweeps")]
        public int Sweeps { get; set; }

        public Record(Transistor vtn,Transistor vtp,
                      decimal time, IEnumerable<decimal> values, string signal, int seed) {
            VtnDeviation = vtn.Deviation;
            VtnSigma = vtn.Sigma;
            VtnVoltage = vtn.Voltage;

            VtpDeviation = vtp.Deviation;
            VtpSigma = vtp.Sigma;
            VtpVoltage = vtp.Voltage;

            Time = time;
            Values = values.ToArray();
            Signal = signal;
            Seed = seed;
            Sweeps = values.Count();
        }

        
        public FilterDefinition<Record> Filter =>
            Builders<Record>.Filter.Where(r =>
                r.Signal == Signal &&
                r.Time == Time &&
                r.VtnDeviation == VtnDeviation &&
                r.VtnSigma == VtnSigma &&
                r.VtnVoltage == VtnVoltage &&
                r.VtpDeviation == VtpDeviation &&
                r.VtpSigma == VtpSigma &&
                r.VtpVoltage == VtpVoltage &&
                r.Seed == Seed &&
                r.Sweeps == Sweeps
            );


        public UpdateDefinition<Record> Update => Builders<Record>.Update.Set(f => f.Values, Values);

        public static FilterDefinition<Record> FindFilter(Transistor vtn, Transistor vtp, string signal, decimal time,
            int sweeps)
            => Builders<Record>.Filter
                .Where(r =>
                    r.Signal == signal &&
                    r.Time == time &&
                    r.Sweeps == sweeps &&
                    vtn.Voltage == r.VtnVoltage &&
                    vtn.Sigma == r.VtnSigma &&
                    vtn.Deviation == r.VtnDeviation &&
                    vtp.Voltage == r.VtpVoltage &&
                    vtp.Sigma == r.VtpSigma &&
                    vtp.Deviation == r.VtpDeviation
                );

        public override string ToString() {
            return
                $"Vtn: {VtnVoltage},{VtnSigma},{VtnDeviation}, Vtp: {VtpVoltage},{VtpSigma},{VtpDeviation}, Signal: {Signal}, Seed: {Seed}, Sweeps: {Sweeps}, Time: {Time}";
        }
    }
}