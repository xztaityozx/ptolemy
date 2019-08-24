using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.PipeLine {
    public delegate void OnBeginEventHandler();
    public delegate void OnIntervalEventHandler(object obj);
    public delegate void OnFinishEventHandler();
    public delegate void OnInnerIntervalEventHandler(object obj);
    public interface IPipeLineStage :IDisposable {
        void Invoke(CancellationToken token);
        IPipeLineStage Next { get; }
    }
    internal enum Mode {
        Select,
        SelectMany,
        Unknown
    }



    public class PipeLineStage<TSource, TResult> : IPipeLineStage {

        internal BlockingCollection<TSource> Sources;
        internal BlockingCollection<TResult> Results;
        internal Func<TSource, TResult> Filter;
        internal Func<TSource, IEnumerable<TResult>> EnumerableFilter;
        internal Mode Mode = Mode.Unknown;
        internal int Workers;

        public IEnumerable<TResult> Out => Results.GetConsumingEnumerable();

        internal PipeLineStage(int bufferSize) {
            Results = new BlockingCollection<TResult>(bufferSize);
        }

        internal PipeLineStage(IEnumerable<TSource> source, int bufferSize) :this(bufferSize) {
            Sources = new BlockingCollection<TSource>();
            foreach (var s in source) {
                Sources.Add(s);
            }
            Sources.CompleteAdding();
        }

        public IPipeLineStage Next { get; private set; }


        public void Dispose() {
            Sources.Dispose();
            Results.Dispose();
        }

        public event OnBeginEventHandler OnBegin;
        public event OnIntervalEventHandler OnInterval;
        public event OnFinishEventHandler OnFinish;
        public event OnInnerIntervalEventHandler OnInnerInterval;


        /// <summary>
        /// Add next pipeline stage
        /// </summary>
        /// <typeparam name="TNextResult">Next result type</typeparam>
        /// <param name="filter"></param>
        /// <param name="workers"></param>
        /// <param name="queueSize"></param>
        /// <param name="onBeginEvent"></param>
        /// <param name="onFinishEvent"></param>
        /// <param name="onIntervalEvent"></param>
        /// <returns></returns>
        public PipeLineStage<TResult, TNextResult> Then<TNextResult>(
            int workers,
            int queueSize,
            Func<TResult, TNextResult> filter,
            OnBeginEventHandler onBeginEvent = null,
            OnFinishEventHandler onFinishEvent = null,
            OnIntervalEventHandler onIntervalEvent = null) {

            if(workers<=0) throw new PipeLineException("workers must be more than 1");
            if(queueSize<=0) throw new PipeLineException("queueSize must be more than 1");

            var rt = new PipeLineStage<TResult, TNextResult>(queueSize) {
                Workers = workers,
                Sources = Results,
                Filter = filter,
                Mode=Mode.Select
            };
            rt.OnBegin += onBeginEvent;
            rt.OnFinish += onFinishEvent;
            rt.OnInterval += onIntervalEvent;
            Next = rt;

            return rt;
        }

        /// <summary>
        /// Add next split pipeline stage
        /// </summary>
        /// <typeparam name="TNextResult">next result type</typeparam>
        /// <param name="workers"></param>
        /// <param name="queueSize"></param>
        /// <param name="filter"></param>
        /// <param name="onBeginEvent"></param>
        /// <param name="onFinishEvent"></param>
        /// <param name="onIntervalEvent"></param>
        /// <param name="onInnerInterval"></param>
        /// <returns></returns>
        public PipeLineStage<TResult, TNextResult> ThenSelectMany<TNextResult>(
            int workers,
            int queueSize,
            Func<TResult, IEnumerable<TNextResult>> filter,
            OnBeginEventHandler onBeginEvent = null,
            OnFinishEventHandler onFinishEvent = null, 
            OnIntervalEventHandler onIntervalEvent = null,
            OnInnerIntervalEventHandler onInnerInterval = null) {

            if (workers <= 0) throw new PipeLineException("workers must be more than 1");
            if (queueSize <= 0) throw new PipeLineException("queueSize must be more than 1");

            var rt = new PipeLineStage<TResult, TNextResult>(queueSize) {
                Workers = workers,
                Sources = Results,
                EnumerableFilter = filter,
                Mode=Mode.SelectMany
            };
            rt.OnBegin += onBeginEvent;
            rt.OnFinish += onFinishEvent;
            rt.OnInterval += onIntervalEvent;
            rt.OnInnerInterval += onInnerInterval;
            Next = rt;

            return rt;
        }

        /// <summary>
        /// Add buffer to this pipeline
        /// </summary>
        /// <param name="size"></param>
        /// <param name="queueSize"></param>
        /// <param name="onBeginEvent"></param>
        /// <param name="onFinishEvent"></param>
        /// <param name="onIntervalEvent"></param>
        /// <returns></returns>
        public PipeLineBuffer<TResult> Buffer(
            int size, int queueSize,
            OnBeginEventHandler onBeginEvent = null,
            OnFinishEventHandler onFinishEvent = null,
            OnIntervalEventHandler onIntervalEvent = null
        ) {
            var rt = new PipeLineBuffer<TResult>(Results, queueSize, size);
            rt.OnBegin += onBeginEvent;
            rt.OnFinish += onFinishEvent;
            rt.OnInterval += onIntervalEvent;
            Next = rt;

            return rt;
        }

        /// <summary>
        /// Invoke this pipeline stage
        /// </summary>
        /// <param name="token">Token for cancel</param>
        /// <exception cref="OperationCanceledException"></exception>
        public void Invoke(CancellationToken token) {
            // workers
            void Worker() {
                switch (Mode) {
                    case Mode.Select:
                        foreach (var source in Sources.GetConsumingEnumerable()) {
                            token.ThrowIfCancellationRequested();

                            Results.Add(Filter(source), token);
                            OnInterval?.Invoke(source);
                        }
                        break;
                    case Mode.SelectMany:
                        foreach (var source in Sources.GetConsumingEnumerable()) {
                            token.ThrowIfCancellationRequested();

                            foreach (var result in EnumerableFilter(source)) {
                                Results.Add(result, token);
                                OnInnerInterval?.Invoke(result);
                            }
                            OnInterval?.Invoke(source);
                        }
                        break;
                    case Mode.Unknown:
                        throw new PipeLineException("Invalid stage status");
                    default:
                        throw new PipeLineException("Invalid stage status");
                }
            }

            OnBegin?.Invoke();

            // start tasks
            var tasks = new Task[Workers];
            for (var i = 0; i < Workers; i++) {
                tasks[i] = Task.Factory.StartNew(Worker, token);
            }
            Task.WaitAll(tasks);
            Results.CompleteAdding();
            // end tasks

            OnFinish?.Invoke();
        }

    }

}
