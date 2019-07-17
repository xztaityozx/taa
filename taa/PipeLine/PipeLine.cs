using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;

namespace taa.PipeLine {
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
                ProgressCharacter = '=',
                BackgroundCharacter = '-'
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
            parentBar?.Dispose();
            foreach (var f in stages) {
                f?.Dispose();
            }
        }
    }
}