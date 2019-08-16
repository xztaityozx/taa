using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa.PipeLine {
    public delegate void PipeLineFinishEventHandler();
    public delegate void PipeLineIntervalEventHandler(object s);

    public interface IPipeLineStage : IDisposable {
        void Run();
        event PipeLineIntervalEventHandler OnInterval;
        event PipeLineFinishEventHandler OnFinish;
    }

    public enum PipeLineStageMode {
        Select,
        SelectMany,
        Last
    }

    /// <summary>
    /// 各ステージを表すクラス
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class PipeLineStage<TSource, TResult> : IPipeLineStage {
        private readonly CancellationToken token;
        public BlockingCollection<TSource> Sources { get; private set; }
        public BlockingCollection<TResult> Results { get; }

        private readonly Func<TSource, TResult> filter;
        private readonly Func<TSource, IEnumerable<TResult>> manyFilter;
        private readonly Action<TSource> outputAction;

        private readonly int workers;

        /// <summary>
        /// ひとつのTSourceを処理するたびに発火するイベント
        /// </summary>
        public event PipeLineIntervalEventHandler OnInterval;

        /// <summary>
        /// このすべてのTSourceを処理したときに発火するイベント
        /// </summary>
        public event PipeLineFinishEventHandler OnFinish;

        public PipeLineStageMode Mode { get; }

        private PipeLineStage(CancellationToken token, int workers, int bufferSize) {
            this.token = token;
            this.workers = workers;
            Results=new BlockingCollection<TResult>(bufferSize);
        }

        private void BuildSource(IEnumerable<TSource> sources) {
            Sources=new BlockingCollection<TSource>();
            foreach (var source in sources) {
                Sources.TryAdd(source);
            }
            Sources.CompleteAdding();
        }

        public PipeLineStage(CancellationToken token, int workers, int bufferSize, BlockingCollection<TSource> sources,
            Func<TSource, TResult> filter,
            Action<TSource> outputAction) :this(token, workers, bufferSize) {
            Sources = sources;
            this.filter = filter;
            this.outputAction = outputAction;
            Mode = PipeLineStageMode.Select;
        }

        public PipeLineStage(CancellationToken token, int workers, int bufferSize, IEnumerable<TSource> sources,
            Func<TSource, TResult> filter) : this(token, workers, bufferSize) {
            this.filter = filter;

            BuildSource(sources);

            Mode = PipeLineStageMode.Select;
        }

        public PipeLineStage(CancellationToken token, int workers, int bufferSize, IEnumerable<TSource> sources,
            Func<TSource, IEnumerable<TResult>> filter) : this(token, workers, bufferSize) {
            manyFilter = filter;
            BuildSource(sources);
            Mode = PipeLineStageMode.SelectMany;
        }

        public PipeLineStage(CancellationToken token, int workers, int bufferSize, BlockingCollection<TSource> sources,
            Func<TSource, IEnumerable<TResult>> filter) : this(token, workers, bufferSize) {
            manyFilter = filter;
            Sources = sources;
            Mode = PipeLineStageMode.SelectMany;
        }

        public PipeLineStage(CancellationToken token, int workers, int bufferSize, BlockingCollection<TSource> sources,
            Action<TSource> action) : this(token, workers, bufferSize) {
            outputAction = action ?? (s => Console.WriteLine(s));
            Sources = sources;
            Mode = PipeLineStageMode.Last;
        }

        public void Run() {
            var tasks = new Task[workers];
            for (var i = 0; i < workers; i++) {
                tasks[i] = Task.Run(Worker, token);
            }

            Task.WaitAll(tasks, token);
            Results.CompleteAdding();

            OnFinish?.Invoke();
        }


        private void Worker() {
            foreach (var source in Sources.GetConsumingEnumerable()) {
                switch (Mode) {
                    case PipeLineStageMode.Last:
                        outputAction(source);
                        break;
                    case PipeLineStageMode.SelectMany: {
                        foreach (var result in manyFilter(source)) {
                            Results.TryAdd(result);
                        }

                        break;
                    }

                    case PipeLineStageMode.Select:
                        Results.TryAdd(filter(source));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                OnInterval?.Invoke(source);
            }
        }

        public void Dispose() {
            Sources?.Dispose();
            Results?.Dispose();
        }
    }
}