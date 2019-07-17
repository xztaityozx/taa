using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa.PipeLine {

    public interface IPipeLineStage : IDisposable {
        void Run();
    }

    /// <summary>
    /// 各ステージを表すクラス
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class PipeLineStage<TSource, TResult> : IPipeLineStage {
        private readonly CancellationToken token;
        public readonly BlockingCollection<TSource> Sources;
        public readonly BlockingCollection<TResult> Results;

        private readonly Func<TSource, TResult> filter;
        private readonly Action<TSource> outputAction;

        private readonly IProgressBar bar, parentBar;
        private readonly int workers;

        private readonly string name;

        private bool IsLastFilter => filter == null && outputAction != null;

        public void Run() {
            var tasks = new Task[workers];
            for (var i = 0; i < workers; i++) {
                tasks[i] = Task.Run(Worker, token);
            }

            Task.WaitAll(tasks, token);
            Results.CompleteAdding();
        }


        private void Worker() {
            foreach (var source in Sources.GetConsumingEnumerable()) {
                if (IsLastFilter) outputAction(source);
                else {
                    var res = filter(source);
                    Results.TryAdd(res);
                }
                bar.Tick(name);
                parentBar.Tick();
            }

        }

        public PipeLineStage(CancellationToken token, IEnumerable<TSource> source, IProgressBar bar, IProgressBar parentBar,
            int workers, Func<TSource,TResult> filter) :
            this(token, bar, parentBar, workers, filter) {
            Sources = new BlockingCollection<TSource>();
            foreach (var s in source) {
                Sources.TryAdd(s);
            }

            Sources.CompleteAdding();
        }

        private PipeLineStage(CancellationToken token, IProgressBar bar, IProgressBar parentBar,
            int workers) {
            this.token = token;
            Results = new BlockingCollection<TResult>();

            this.bar = bar;
            this.workers = workers;
            this.parentBar = parentBar;

            name = bar.Message;
        }

        private PipeLineStage(CancellationToken token, IProgressBar bar, IProgressBar parentBar,
            int workers, Func<TSource, TResult> filter) : this(token,
            bar,parentBar, workers) {
            this.filter = filter;
        }

        private PipeLineStage(CancellationToken token, IProgressBar bar, IProgressBar parentBar,
            int workers, Action<TSource> action) : this(token, bar,parentBar,workers) {
            outputAction = action;
        }

        public PipeLineStage(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar,
            IProgressBar parentBar,
            int workers,
            Func<TSource, TResult> filter) : this(token, bar, parentBar, workers, filter) {
            Sources = sources;
        }

        public PipeLineStage(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar,
            IProgressBar parentBar,
            int workers, Action<TSource> action) : this(token, bar, parentBar, workers,action) {
            Sources = sources;
        }

        public void Dispose() {
            Sources?.Dispose();
            Results?.Dispose();
            bar?.Dispose();
        }
    }
}