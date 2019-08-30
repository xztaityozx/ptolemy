using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kurukuru;

namespace Ptolemy.PipeLine {
    public abstract class PipeLineStageBase<TSource,TResult> : IPipeLineStage {
        protected BlockingCollection<TSource> Sources;
        protected BlockingCollection<TResult> Results;
        internal readonly int Workers;
        protected OnBeginEventHandler OnBegin;
        protected OnFinishEventHandler OnFinish;
        protected OnIntervalEventHandler OnInterval;

        public void Dispose() {
            Sources?.Dispose();
            Results?.Dispose();
        }

        public IEnumerable<TResult> Out => Results.GetConsumingEnumerable();

        public PipeLineStageBase<TResult, TNextResult> Then<TNextResult>(
            int workers,
            int queueSize,
            Func<TResult, TNextResult> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null
        ) {
            var rt = new PipeLineSelectStage<TResult, TNextResult>(Results, workers, queueSize, filter, onBegin, onFinish, onInterval);
            Next = rt;
            return rt;
        }

        public PipeLineStageBase<TResult, TNextResult> ThenSelectMany<TNextResult>(
            int workers,
            int queueSize,
            Func<TResult, IEnumerable<TNextResult>> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null,
            OnInnerIntervalEventHandler onInnerInterval = null
        ) {
            var rt = new PipeLineManyStage<TResult, TNextResult>(Results, workers, queueSize, filter, onBegin, onFinish,
                onInterval, onInnerInterval);
            Next = rt;
            return rt;
        }

        public PipeLineStageBase<TResult, TResult[]> Buffer(
            int size,
            int queueSize,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null
        ) {
            var rt = new PipeLineBuffer<TResult>(Results, size, 1, queueSize, onBegin, onFinish, onInterval);
            Next = rt;
            return rt;
        }

        private PipeLineStageBase(int workers, int queueSize) {
            if (workers <= 0) throw new PipeLineException("Invalid worker number");
            if (queueSize <= 0) throw new PipeLineException("Invalid queueSize number");

            Workers = workers;
            Results=new BlockingCollection<TResult>(queueSize);
        }

        internal PipeLineStageBase(
            BlockingCollection<TSource> sources,
            int workers,
            int queueSize
        ) : this(workers, queueSize) {
            Sources = sources;
        }

        public abstract void Invoke(CancellationToken token);

        public IPipeLineStage Next { get; private set; }
    }
}
