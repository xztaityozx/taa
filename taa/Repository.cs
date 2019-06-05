using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            if (!collections.Contains(ParameterCollectionName)) db.CreateCollection(ParameterCollectionName);
            if (!collections.Contains(RecordCollectionName)) db.CreateCollection(RecordCollectionName);
        }

        public IEnumerable<string> Push(Transistor vtn, Transistor vtp, int sweeps, Record[] records) {
            var p = new Parameter {
                Vtn = vtn,
                Vtp = vtp,
                Sweeps = sweeps
            };

            yield return "Updating...";

            // パラメータを探す、なければInsert

            p.Id = FindParameter(vtn, vtp, sweeps).Id;


            // データをPushする
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

        private Parameter FindParameter(Transistor vtn, Transistor vtp, int sweeps) {
            var col = db.GetCollection<Parameter>(ParameterCollectionName);

            return col.FindOneAndUpdate(
                Builders<Parameter>.Filter.Where(r =>
                    r.Sweeps == sweeps && r.Vtn == vtn && r.Vtp == vtp),
                Builders<Parameter>.Update
                    .Set(r => r.Sweeps, sweeps)
                    .Set(r => r.Vtn, vtn)
                    .Set(r => r.Vtp, vtp), new FindOneAndUpdateOptions<Parameter> {
                    // Upsertを指定
                    IsUpsert = true,
                    // UpsertしたDocumentを返す（あとにIDが必要なため）
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        private Parameter FindParameter(Request request) => FindParameter(request.Vtn, request.Vtp, request.Sweeps);

        public IEnumerable<Record> Pull(Request request, IProgressBar pb) {
            var id = FindParameter(request).Id;

            var recordCollection = db.GetCollection<Record>(RecordCollectionName);
            var tasks = request.FindFilterDefinitions(id)
                .Select(findFilterDefinition =>
                    recordCollection.FindAsync(findFilterDefinition))
                .ToList();

            return Task.WhenAll(tasks).ContinueWith(t => {
                    pb?.Tick();
                    return t.Result.SelectMany(k => k.ToList());
                })
                .Result;
        }
    }
}