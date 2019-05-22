using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace taa {
    public class Repository {
        private readonly DatabaseConfig config;
        private readonly MongoClient client;
        private readonly IMongoDatabase db;

        public Repository(DatabaseConfig config) {
            this.config = config;
            client = new MongoClient(config.ToString());
            db = client.GetDatabase(config.Name);

            if (db.ListCollectionNames().ToList().All(c => c != config.Collection)) {
                db.CreateCollection(config.Collection);
            }
        }

        public void PushMany(IEnumerable<Record> records) {
            var collection = db.GetCollection<Record>(config.Collection);

            var requests = records.Select(record => new UpdateOneModel<Record>(record.Filter, record.Update){IsUpsert = true}).ToList();
            collection.BulkWrite(requests);
        }

        public class Record {
            public decimal VtnVoltage { get; }
            public decimal VtnSigma { get; }
            public decimal VtnDeviation { get; }
            public decimal VtpVoltage { get; }
            public decimal VtpSigma { get; }
            public decimal VtpDeviation { get; }
            public decimal Time { get; }
            public decimal[] Values { get; } 
            public string Signal { get; }

            public Record(decimal  vtnVoltage, decimal vtnSigma, decimal vtnDeviation, 
                          decimal vtpVoltage, decimal vtpSigma, decimal vtpDeviation,
                          decimal time, IEnumerable<decimal> values, string signal) {
                VtnDeviation = vtnDeviation;
                VtnSigma = vtnSigma;
                VtnVoltage = vtnVoltage;
                
                VtpDeviation = vtpDeviation;
                VtpSigma = vtpSigma;
                VtpVoltage = vtpVoltage;

                Time = time;
                Values = values.ToArray();
                Signal = signal;
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
                    r.VtpVoltage == VtpVoltage
                );
            

            public UpdateDefinition<Record> Update => Builders<Record>.Update.Set(f => f.Values, Values);
            
        }
    }
}