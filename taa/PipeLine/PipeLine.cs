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
        private readonly CancellationToken token;

        public enum PipeLineState {
            Completed,
            Canceled,
            Failed,
            Unknown
        }

        public PipeLine(CancellationToken token) {
            this.token = token;
            stages = new List<IPipeLineStage>();
        }

        

        public PipeLineStage<TSource, TResult> Add<TSource,TResult>(int workers,int bufferSize, BlockingCollection<TSource> sources, 
            Func<TSource, TResult> filter) {
            var f = new PipeLineStage<TSource, TResult>(token, workers, bufferSize,sources, filter);
            stages.Add(f);
            return f;
        }

        public PipeLineStage<TSource, TResult> Add<TSource, TResult>(int workers, int bufferSize,IEnumerable<TSource> sources,
            Func<TSource, TResult> filter) {
            var f = new PipeLineStage<TSource,TResult>(token, workers, bufferSize,sources, filter);
            stages.Add(f);
            return f;
        }

        public void Add<TSource, TResult>(int workers, int bufferSize,BlockingCollection<TSource> sources,
            Action<TSource> action) {
            var f = new PipeLineStage<TSource,TResult>(token, workers, bufferSize,sources, action);
            stages.Add(f);
        }

        public PipeLineStage<TSource, TResult> AddSelectMany<TSource, TResult>(int workers, int bufferSize,BlockingCollection<TSource> sources,
            Func<TSource, IEnumerable<TResult>> filter) {
            var f = new PipeLineStage<TSource,TResult>(token, workers, bufferSize,sources, filter);
            stages.Add(f);
            return f;
        }

        public PipeLineStage<TSource,TResult> AddSelectMany<TSource, TResult>(int workers, int bufferSize,IEnumerable<TSource> sources,
            Func<TSource, IEnumerable<TResult>> filter) {
            var f = new PipeLineStage<TSource, TResult>(token, workers, bufferSize,sources, filter);
            stages.Add(f);
            return f;
        }


        /// <summary>
        /// 処理を開始する
        /// </summary>
        /// <returns>パイプラインの終了ステータス</returns>
        public PipeLineState Invoke(Action aggregator) {
            var tasks = new Task[stages.Count+1];

            for (var i = 0; i < stages.Count; i++) {
                tasks[i] = Task.Run(stages[i].Run, token);
            }

            tasks[stages.Count] = Task.Factory.StartNew(aggregator, token);

            Task.WaitAll(tasks, token);

            if (tasks.Any(t => t.IsCanceled)) return PipeLineState.Canceled;
            if (tasks.Any(t => t.IsFaulted)) return PipeLineState.Failed;
            return tasks.All(t => t.IsCompletedSuccessfully) ? PipeLineState.Completed : PipeLineState.Unknown;
        }

        public void Dispose() {
            foreach (var f in stages) {
                f?.Dispose();
            }
        }
    }
}