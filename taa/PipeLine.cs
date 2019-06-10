using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa {
    public class PipeLine {
        private readonly List<IPipeLineFilter> filters;

        public PipeLine() {
            filters=new List<IPipeLineFilter>();
        }

        public void Add(IPipeLineFilter filter) {
            filters.Add(filter);
        }

    }

    public interface IPipeLineFilter {
        void Run();
    }

    public class PipeLineFilter<TSource, TResult> : IPipeLineFilter {
        private readonly BlockingCollection<TSource> sources;
        public readonly BlockingCollection<TResult> Destination;
        private readonly Func<TSource, TResult> filter;
        private readonly Action<TSource> outputAction;
        public int Parallel { get; }
        public string Name { get; }
        private readonly Logger.Logger logger;
        private readonly CancellationToken token;

        public void Run() {

            var tasks = new List<Task>();
            for (var i = 0; i < Parallel; i++) {
                tasks.Add(Task.Run(Worker, token));
            }
            
            logger.Info($"{Name}: {Parallel} worker running...");
            Task.WhenAll(tasks);
            Destination.CompleteAdding();
            logger.Info($"{Name}: Finished");
        }

        private void Worker() {
            foreach (var source in sources) {
                if (IsLastFilter) outputAction(source);
                else Destination.TryAdd(filter(source));
            }
        }


        public PipeLineFilter(
            BlockingCollection<TSource> sources,
            BlockingCollection<TResult> destination,
            string name,
            int size,
            Logger.Logger logger,
            CancellationToken token
        ) {
            this.sources = sources;
            this.Destination = destination;
            Name = name;
            this.logger = logger;
            this.token = token;
            Parallel = size;
        }

        public PipeLineFilter(
            Func<TSource, TResult> filter,
            BlockingCollection<TSource> sources,
            BlockingCollection<TResult> destination,
            int size,
            string name,
            Logger.Logger logger,
            CancellationToken token
        ) :this(sources,destination,name,size,logger,token) {
            this.filter = filter;
        }
        
        public PipeLineFilter(
            BlockingCollection<TSource> sources,
            Action<TSource> action,
            string name,
            Logger.Logger logger,
            CancellationToken token
        ):this(sources,null,name,1,logger,token) {
            outputAction = action;
        }

        private bool IsLastFilter => outputAction != null && filter != null;
    }
}