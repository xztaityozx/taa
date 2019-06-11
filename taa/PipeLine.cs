using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa {
    public class PipeLine :IDisposable {
        private readonly List<IPipeLineFilter> filters;
        private readonly IProgressBar parentBar;
        private readonly CancellationToken token;

        private readonly ProgressBarOptions childOption = new ProgressBarOptions {
            ProgressCharacter = '>',
            ForegroundColor = ConsoleColor.DarkCyan
        };
        
        public PipeLine(CancellationToken token, int steps) {
            filters = new List<IPipeLineFilter>();
            this.token = token;
            parentBar = new ProgressBar(steps, "Master");
        }

        public BlockingCollection<TResult> Add<TSource, TResult>(string name, int workers, IEnumerable<TSource> sources , int size, Func<TSource, TResult> filter) {
            var pb = parentBar.Spawn(size, name, childOption);
            var f = new PipeLineFilter<TSource, TResult>(
                token, sources, pb, workers, filter
            );

            filters.Add(f);
            return f.Results;
        }

        public BlockingCollection<TResult> Add<TSource, TResult>(string name, int workers,
            BlockingCollection<TSource> sources, int size, Func<TSource, TResult> filter) {
            var f = new PipeLineFilter<TSource, TResult>(
                token, sources, parentBar.Spawn(size, name, childOption), workers, filter
            );

            filters.Add(f);
            return f.Results;
        }

        public void Add<TSource, TResult>(string name, int workers,
            BlockingCollection<TSource> sources, int size, Action<TSource> action) {

            var f = new PipeLineFilter<TSource, TSource>(
                token, sources, parentBar.Spawn(size, name, childOption), workers, action
            );
            
            filters.Add(f);
        }


        public void Invoke() {
            filters.AsParallel()
                .WithCancellation(token)
                .WithDegreeOfParallelism(filters.Count)
                .ForAll(f => f.Run());
        }

        public void Dispose() {
            parentBar.Dispose();
            foreach (var f in filters) {
                f.Dispose();
            }
        }
    }

    public interface IPipeLineFilter : IDisposable {
        void Run();
    }

    public class PipeLineFilter<TSource, TResult> : IPipeLineFilter {
        private readonly CancellationToken token;
        public readonly BlockingCollection<TSource> sources;
        public readonly BlockingCollection<TResult> Results;

        private readonly Func<TSource, TResult> filter;
        private readonly Action<TSource> outputAction;

        public int ProcessedTasks { get; private set; }

        private readonly IProgressBar bar;
        private readonly int workers;

        private bool IsLastFilter => filter == null && outputAction != null;

        public void Run() {
            var tasks = new List<Task<int>>();
            for (var i = 0; i < workers; i++) {
                tasks.Add(Task.Run(Worker, token));
            }

            ProcessedTasks = Task.WhenAll(tasks).Result.Sum();
            Results.CompleteAdding();
        }

        private int Worker() {
            var rt = 0;
            foreach (var source in sources.GetConsumingEnumerable()) {
                if (IsLastFilter) outputAction(source);
                else {
                    var res = filter(source);
                    Results.Add(res, token);
                }

                bar.Tick();
                rt++;
            }

            return rt;
        }

        public PipeLineFilter(CancellationToken token, IEnumerable<TSource> source, IProgressBar bar, int workers, Func<TSource,TResult> filter) :
            this(token, bar, workers, filter) {
            this.sources = new BlockingCollection<TSource>();
            foreach (var s in source) {
                sources.Add(s, token);
            }

            sources.CompleteAdding();
        }

        private PipeLineFilter(CancellationToken token, IProgressBar bar, int workers) {
            this.token = token;
            Results = new BlockingCollection<TResult>();

            this.bar = bar;
            this.workers = workers;
        }

        private PipeLineFilter(CancellationToken token, IProgressBar bar,int workers, Func<TSource, TResult> filter) : this(token,
            bar, workers) {
            this.filter = filter;
        }

        private PipeLineFilter(CancellationToken token, IProgressBar bar, int workers, Action<TSource> action) : this(token, bar,workers) {
            this.outputAction = action;
        }

        public PipeLineFilter(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar, int workers,
            Func<TSource, TResult> filter) : this(token, bar, workers, filter) {
            this.sources = sources;
        }

        public PipeLineFilter(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar,
            int workers, Action<TSource> action) : this(token, bar, workers,action) {
            this.sources = sources;
        }

        public void Dispose() {
            sources?.Dispose();
            Results?.Dispose();
            bar?.Dispose();
        }
    }
}