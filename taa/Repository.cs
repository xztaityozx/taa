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

        public Repository(DatabaseConfig config) {
            this.config = config;
            var client = new MongoClient(config.ToString());
            db = client.GetDatabase(config.DataBaseName);

            if (db.ListCollectionNames().ToList().All(c => c != config.CollectionName)) {
                db.CreateCollection(config.CollectionName);
            }
        }
    }
}