using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.PipeLine {
    public class PipeLine : IDisposable {
        public enum PipeLineStatus {
            Completed,
            Faulted,
            Unknown
        }

        /// <summary>
        /// Start pipeline
        /// </summary>
        /// <param name="lastAction"></param>
        /// <returns></returns>
        /// <exception cref="PipeLineException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public PipeLineStatus Start(Action lastAction) {
            if (stages.Count == 0) throw new PipeLineException("pipeline must be have stage at least one");
            var stage = stages[0];

            if (stage == null) throw new PipeLineException("The first stage can not be null");
            while (stage.Next != null) {
                stage = stage.Next;
                stages.Add(stage);
            }

            var tasks = new List<Task>();
            if (lastAction != null) tasks.Add(Task.Factory.StartNew(lastAction, token));
            foreach (var pipeLineStage in stages) {
                tasks.Add(Task.Factory.StartNew(() => pipeLineStage.Invoke(token), token));
            }

            try {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e) {
                foreach (var exception in e.InnerExceptions) {
                    if (exception is TaskCanceledException) throw exception;
                }
            }
            if (tasks.Any(t => t.IsFaulted)) return PipeLineStatus.Faulted;
            return tasks.All(t => t.IsCompletedSuccessfully || t.IsCompleted)
                ? PipeLineStatus.Completed
                : PipeLineStatus.Unknown;

        }

        private readonly List<IPipeLineStage> stages;
        private readonly CancellationToken token;

        public PipeLine(CancellationToken token) {
            stages = new List<IPipeLineStage>();
            this.token = token;
        }

        /// <summary>
        /// Add first stage to this pipeline
        /// </summary>
        /// <typeparam name="TSource">Input type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="workers">Number of workers</param>
        /// <param name="bufferSize">Size of result buffer</param>
        /// <param name="filter"></param>
        /// <param name="onBegin"></param>
        /// <param name="onFinish"></param>
        /// <param name="onInterval"></param>
        /// <returns></returns>
        public PipeLineStage<TSource, TResult> Init<TSource, TResult>(
            IEnumerable<TSource> source,
            int workers,
            int bufferSize,
            Func<TSource, TResult> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null) {

            if (workers <= 0) throw new PipeLineException("workers must be more than 1");
            if (bufferSize <= 0) throw new PipeLineException("bufferSize must be more than 1");


            var rt = new PipeLineStage<TSource, TResult>(source, bufferSize) {
                Workers = workers,
                Filter = filter,
                Mode = Mode.Select
            };

            rt.OnBegin += onBegin;
            rt.OnInterval += onInterval;
            rt.OnFinish += onFinish;

            // first stage
            stages.Add(rt);

            return rt;
        }

        /// <summary>
        /// Add first split stage to this pipeline
        /// </summary>
        /// <typeparam name="TSource">Input type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="workers">Number of workers</param>
        /// <param name="bufferSize">Size of result buffer</param>
        /// <param name="filter"></param>
        /// <param name="onBegin"></param>
        /// <param name="onFinish"></param>
        /// <param name="onInterval"></param>
        /// <param name="onInnerInterval"></param>
        /// <returns></returns>
        public PipeLineStage<TSource, TResult> InitSelectMany<TSource, TResult>(
            IEnumerable<TSource> source,
            int workers,
            int bufferSize,
            Func<TSource, IEnumerable<TResult>> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null,
            OnInnerIntervalEventHandler onInnerInterval = null) {

            if (workers <= 0) throw new PipeLineException("workers must be more than 1");
            if (bufferSize <= 0) throw new PipeLineException("bufferSize must be more than 1");

            var rt = new PipeLineStage<TSource, TResult>(bufferSize) {
                Workers = workers,
                Sources = new BlockingCollection<TSource>(),
                EnumerableFilter = filter,
                Mode = Mode.SelectMany
            };
            foreach (var s in source) {
                rt.Sources.Add(s, token);
            }

            rt.Sources.CompleteAdding();
            rt.OnBegin += onBegin;
            rt.OnInterval += onInterval;
            rt.OnFinish += onFinish;
            rt.OnInnerInterval += onInnerInterval;

            // first stage
            stages.Add(rt);

            return rt;
        }

        public void Dispose() {
            foreach (var pipeLineStage in stages) {
                pipeLineStage.Dispose();
            }
        }
    }

    public class PipeLineException : Exception {
        public PipeLineException(string msg) : base(msg) {
        }
    }
}
