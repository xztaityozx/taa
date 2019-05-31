using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    public class Repository {
        private readonly DatabaseConfig config;
        private readonly IMongoDatabase db;
        private const string RecordCollectionName = "records";
        private const string ParameterCollectionName = "parameters";

        public Repository(DatabaseConfig config) {
            this.config = config;
            var client = new MongoClient(config.ToString());
            db = client.GetDatabase(config.DataBaseName);

            var collections = db.ListCollectionNames().ToList();
            if(!collections.Contains(ParameterCollectionName)) db.CreateCollection(ParameterCollectionName);
            if (!collections.Contains(RecordCollectionName)) db.CreateCollection(RecordCollectionName);
        }

        // TODO: Exception occured!!!!!!!!!!
        public IEnumerable<string> Push(Transistor vtn, Transistor vtp, int sweeps, Record[] records) {
            var parameterCollection = db.GetCollection<Parameter>(ParameterCollectionName);
            var p = new Parameter {
                Vtn = vtn,
                Vtp = vtp,
                Sweeps = sweeps
            };

            yield return "Updating...";

            var res = parameterCollection.FindOneAndUpdate(
                Builders<Parameter>.Filter.Where(r =>
                    r.Sweeps == sweeps && r.Vtn == p.Vtn && r.Vtp == p.Vtp),
                Builders<Parameter>.Update
                    .Set(r => r.Sweeps, p.Sweeps)
                    .Set(r => r.Vtn, p.Vtn)
                    .Set(r => r.Vtp, p.Vtp), new FindOneAndUpdateOptions<Parameter>{IsUpsert = true}
            );

            p.Id = res.Id;
            

            yield return "Writing...";
            var recordCollection = db.GetCollection<Record>(RecordCollectionName);
            recordCollection.BulkWrite(
                records.Select(r => new UpdateOneModel<Record>(
                    Builders<Record>.Filter.Where(k =>
                        k.ParameterId == p.Id &&
                        k.Key == r.Key &&
                        k.Seed == r.Seed
                    ),
                    Builders<Record>.Update
                        .Set(k => k.Values, r.Values)) {IsUpsert = true})
            );
        }

        public IEnumerable<Record> Pull(Request request, ChildProgressBar pb) {
            var parameterCollection = db.GetCollection<Parameter>(ParameterCollectionName);
            var id = parameterCollection.FindSync(
                Builders<Parameter>.Filter.Where(r =>
                    r.Sweeps == request.Sweeps && r.Vtn == request.Vtn && r.Vtp == request.Vtp)
            ).ToList().First().Id;

            var recordCollection = db.GetCollection<Record>(RecordCollectionName);
            var tasks = request.FindFilterDefinitions(id)
                .Select(findFilterDefinition =>
                    recordCollection.FindAsync(findFilterDefinition))
                .ToList();

            return Task.WhenAll(tasks).ContinueWith(t =>
                    t.Result.SelectMany(k => {
                        pb?.Tick();
                        return k.ToList();
                    }))
                .Result;
        }
    }
}