﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ptolemy.PipeLine
{
    public class PipeLineManyStage<TSource, TResult> : PipeLineStageBase<TSource, TResult> {
        private readonly Func<TSource, IEnumerable<TResult>> filter;
        public OnInnerIntervalEventHandler OnInnerInterval;

        public PipeLineManyStage(
            BlockingCollection<TSource> sources, 
            int workers, 
            int queueSize, 
            Func<TSource, IEnumerable<TResult>> filter,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null,
            OnInnerIntervalEventHandler onInnerInterval = null
            ) : base(sources, workers, queueSize) {
            this.filter = filter;
            OnBegin += onBegin;
            OnFinish += onFinish;
            OnInterval += onInterval;
            OnInnerInterval += onInnerInterval;
        }

        public override void Invoke(CancellationToken token) {
            foreach (var source in Sources.GetConsumingEnumerable()) {

                token.ThrowIfCancellationRequested();

                foreach (var result in filter(source)) {
                    Results.Add(result, token);
                }
            }
            Results.CompleteAdding();
        }
    }
}
