using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logger;
using ShellProgressBar;

namespace taa {
    /// <summary>
    /// いくつかのステージからなるパイプライン処理をする
    /// </summary>
    public class PipeLine :IDisposable {
        // Filters
        private readonly List<IPipeLineStage> stages;
        private readonly IProgressBar parentBar;
        private readonly CancellationToken token;

        // 子プログレスバーのオプション
        private readonly ProgressBarOptions childOption = new ProgressBarOptions {
            ProgressCharacter = '>',
            ForegroundColor = ConsoleColor.DarkCyan
        };
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="total">すべてのパイプラインで処理されるオブジェクトの合計</param>
        public PipeLine(CancellationToken token, int total) {
            stages = new List<IPipeLineStage>();
            this.token = token;
            parentBar = new ProgressBar(total, "Master", new ProgressBarOptions {
                ForegroundColor = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '█',
                BackgroundCharacter = '▒'
            });
        }

        /// <summary>
        /// ステージを追加する
        /// </summary>
        /// <typeparam name="TSource">入力の型</typeparam>
        /// <typeparam name="TResult">出力の型</typeparam>
        /// <param name="name">ステージの名前</param>
        /// <param name="workers">このステージの並列数</param>
        /// <param name="sources">ソースになるコレクション</param>
        /// <param name="filter">このステージの具体的な処理</param>
        /// <returns>パイプになるBlockingCollection<TResult></returns>
        public BlockingCollection<TResult> Add<TSource, TResult>(string name, int workers, TSource[] sources , Func<TSource, TResult> filter) {
            var pb = parentBar.Spawn(sources.Count(), name, childOption);
            var f = new PipeLineStage<TSource, TResult>(
                token, sources, pb, parentBar, workers, filter
            );

            stages.Add(f);
            return f.Results;
        }

        /// <summary>
        /// ステージを追加する
        /// </summary>
        /// <typeparam name="TSource">入力の型</typeparam>
        /// <typeparam name="TResult">出力の型</typeparam>
        /// <param name="name">ステージの名前</param>
        /// <param name="workers">このステージの並列数</param>
        /// <param name="sources">ソースになるBlockingCollection<TSource></param>
        /// <param name="size">このコレクションのサイズ</param>
        /// <param name="filter">このステージの具体的な処理</param>
        /// <returns>パイプになるBlockingCollection</TResult></returns>
        public BlockingCollection<TResult> Add<TSource, TResult>(string name, int workers,
            BlockingCollection<TSource> sources, int size, Func<TSource, TResult> filter) {
            var f = new PipeLineStage<TSource, TResult>(
                token, sources, parentBar.Spawn(size, name, childOption), parentBar, workers, filter
            );

            stages.Add(f);
            return f.Results;
        }

        /// <summary>
        /// 出力ステージを追加する
        /// </summary>
        /// <typeparam name="TSource">入力の型</typeparam>
        /// <param name="name">ステージ名</param>
        /// <param name="workers">このステージの並列数</param>
        /// <param name="sources">ソースになるBlockingCollection<TSource></param>
        /// <param name="size">コレクションのサイズ</param>
        /// <param name="action">出力の処理</param>
        public void Add<TSource>(string name, int workers,
            BlockingCollection<TSource> sources, int size, Action<TSource> action) {

            var f = new PipeLineStage<TSource, TSource>(
                token, sources, parentBar.Spawn(size, name, childOption), parentBar, workers, action
            );
            
            stages.Add(f);
        }

        public enum PipeLineState {
            Completed,
            Canceled,
            Failed,
            Unknown
        }

        /// <summary>
        /// 処理を開始する
        /// </summary>
        /// <param name="finishAction"></param>
        /// <returns>パイプラインの終了ステータス</returns>
        public PipeLineState Invoke(Action finishAction) {
            var tasks = new Task[stages.Count + 1];

            for (var i = 0; i < stages.Count; i++) {
                tasks[i] = Task.Run(stages[i].Run, token);
            }

            tasks[stages.Count] = Task.Run(finishAction, token);

            Task.WaitAll(tasks, token);

            if (tasks.Any(t => t.IsCanceled)) return PipeLineState.Canceled;
            if (tasks.Any(t => t.IsFaulted)) return PipeLineState.Failed;
            return tasks.All(t => t.IsCompletedSuccessfully) ? PipeLineState.Completed : PipeLineState.Unknown;
        }

        public void Dispose() {
            parentBar.Dispose();
            foreach (var f in stages) {
                f.Dispose();
            }
        }
    }

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
            this.Sources = new BlockingCollection<TSource>();
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
            this.outputAction = action;
        }

        public PipeLineStage(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar,
            IProgressBar parentBar,
            int workers,
            Func<TSource, TResult> filter) : this(token, bar, parentBar, workers, filter) {
            this.Sources = sources;
        }

        public PipeLineStage(CancellationToken token, BlockingCollection<TSource> sources, IProgressBar bar,
            IProgressBar parentBar,
            int workers, Action<TSource> action) : this(token, bar, parentBar, workers,action) {
            this.Sources = sources;
        }

        public void Dispose() {
            Sources?.Dispose();
            Results?.Dispose();
            bar?.Dispose();
        }
    }
}