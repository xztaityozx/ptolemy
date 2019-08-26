using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kurukuru;

namespace Ptolemy.PipeLine {
    public delegate void OnBeginEventHandler();
    public delegate void OnIntervalEventHandler(object obj);
    public delegate void OnFinishEventHandler();
    public delegate void OnInnerIntervalEventHandler(object obj);
    public interface IPipeLineStage :IDisposable {
        void Invoke(CancellationToken token);
        IPipeLineStage Next { get; }
    }


    public class PipeLineSelectStage<TSource, TResult> : PipeLineStageBase<TSource,TResult> {
        private readonly Func<TSource, TResult> filter;

        public PipeLineSelectStage(
            BlockingCollection<TSource> sources,
            int workers,
            int queueSize,
            Func<TSource, TResult> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null
        ) : base(sources, workers, queueSize) {
            this.filter = filter;
            OnBegin += onBegin;
            OnFinish += onFinish;
            OnInterval += onInterval;
        }

        public override void Invoke(CancellationToken token) {
            foreach (var source in Sources.GetConsumingEnumerable()) {
                token.ThrowIfCancellationRequested();
                Results.Add(filter(source), token);
            }
            Results.CompleteAdding();
        }
    }

}
