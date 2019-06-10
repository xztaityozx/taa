using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kurukuru;
using MongoDB.Bson;
using MongoDB.Driver;
using ShellProgressBar;

namespace taa {
    using RecordQueue = BlockingCollection<Record[]>;
    using SeedFilePair = Tuple<int, string>;

    public class Dispatcher {
        private readonly Config config;

        public Dispatcher(Config config) => this.config = config;

        public IEnumerable<Tuple<string, long>> DispatchPulling(CancellationToken token, Request request, Filter filter) {

            
            return null;
        }
    }
}