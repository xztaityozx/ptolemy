using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ptolemy.PipeLine {
    public class PipeLineBuffer<TItem> : PipeLineStageBase<TItem, TItem[]> {
        internal int Size;

        internal PipeLineBuffer(
            BlockingCollection<TItem> sources,
            int size,
            int workers,
            int queueSize,
            OnBeginEventHandler onBegin = null,
            OnFinishEventHandler onFinish = null,
            OnIntervalEventHandler onInterval = null
        ) : base(sources,
            workers, queueSize) {
            if (size <= 0) throw new PipeLineException("size must be more than one");
            Size = size;
            OnBegin += onBegin;
            OnFinish += onFinish;
            OnInterval += onInterval;
        }

        public override void Invoke(CancellationToken token) {
            OnBegin?.Invoke();
            var box = new List<List<TItem>> {new List<TItem>()};
            var idx = 0;
            foreach (var item in Sources.GetConsumingEnumerable()) {
                token.ThrowIfCancellationRequested();
                OnInterval?.Invoke(item);

                box[idx].Add(item);
                if (box[idx].Count != Size)continue;

                Results.Add(box[idx].ToArray(), token);
                idx++;
                box.Add(new List<TItem>());
            }

            if (box[idx].Any()) Results.Add(box[idx].ToArray(), token);
            Results.CompleteAdding();
            OnFinish?.Invoke();
        }
    }
}
