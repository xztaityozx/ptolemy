using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ptolemy.PipeLine {
    public class PipeLineBuffer<TItem> : IPipeLineStage {
        internal BlockingCollection<TItem> Sources;
        internal BlockingCollection<TItem[]> Results;
        internal int BufferSize;
        internal OnIntervalEventHandler OnInterval;
        internal OnBeginEventHandler OnBegin;
        internal OnFinishEventHandler OnFinish;

        public IEnumerable<TItem[]> Out => Results.GetConsumingEnumerable();

        internal PipeLineBuffer(BlockingCollection<TItem> s, int rSize, int bSize) {
            Sources = s;
            Results = new BlockingCollection<TItem[]>(rSize);
            BufferSize = bSize;
        }

        /// <summary>
        /// Add Buffer to this pipeline
        /// </summary>
        /// <param name="size">Size of BufferSize</param>
        /// <param name="queueSize">Size of Blocking Queue</param>
        /// <param name="onBeginEvent"></param>
        /// <param name="onFinishEvent"></param>
        /// <param name="onIntervalEvent"></param>
        /// <returns></returns>
        public PipeLineBuffer<TItem[]> Buffer(
            int size, int queueSize,
            OnBeginEventHandler onBeginEvent = null,
            OnFinishEventHandler onFinishEvent = null,
            OnIntervalEventHandler onIntervalEvent = null
        ) {
            var rt = new PipeLineBuffer<TItem[]>(Results, queueSize, size);
            rt.OnBegin += onBeginEvent;
            rt.OnFinish += onFinishEvent;
            rt.OnInterval += onIntervalEvent;
            Next = rt;
            return rt;
        }

        public PipeLineStage<TItem[], TResult> Then<TResult>(
            int workers,
            int queueSize,
            Func<TItem[], TResult> filter,
            OnBeginEventHandler onBeginEvent = null,
            OnFinishEventHandler onFinishEvent = null,
            OnIntervalEventHandler onIntervalEvent = null) {
            var rt = new PipeLineStage<TItem[], TResult>(queueSize) {
                Filter = filter,
                Sources = Results,
                Workers = workers,
                Mode = Mode.Select
            };
            rt.OnBegin += onBeginEvent;
            rt.OnFinish += onFinishEvent;
            rt.OnInterval += onIntervalEvent;
            Next = rt;
            return rt;
        }


        public void Dispose() {
            Sources.Dispose();
            Results.Dispose();
        }

        public void Invoke(CancellationToken token) {
            var buffer = new List<List<TItem>> {new List<TItem>()};
            var idx = 0;
            foreach (var item in Sources.GetConsumingEnumerable()) {
                token.ThrowIfCancellationRequested();
                buffer[idx].Add(item);

                if (buffer[idx].Count != BufferSize) continue;

                Results.Add(buffer[idx].ToArray(), token);
                idx++;
                buffer.Add(new List<TItem>());
            }

            if (buffer[idx].Count != 0) Results.Add(buffer[idx].ToArray(), token);

            Results.CompleteAdding();
        }

        public IPipeLineStage Next { get; private set; }
    }
}
