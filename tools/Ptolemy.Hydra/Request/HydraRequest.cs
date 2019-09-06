using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Hydra.Request {
    public class HydraRequest {
        public SimulationRequest BaseRequest { get; set; }
        public Range<long> Seed { get; set; }
        public Range<decimal> Sigma { get; set; }
        public long TotalSweeps { get; set; }
        public SweepSplitOption SweepSplitOption { get; set; }
        public bool NotifyToSlackOnFinished { get; set; }
        public string SlackUserName { get; set; }
        public Guid Id { get; set; }
        public long SweepSplitSize { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
        public static HydraRequest FromJson(string json) => JsonConvert.DeserializeObject<HydraRequest>(json);
        public static HydraRequest FromJson(StreamReader sr) => FromJson(sr.ReadToEnd());
    }

    public enum SweepSplitOption {
        NoSplit,
        SplitBySweep,
        SplitBySeed
    }

    public class Range<T>  {
        public T Start { get; set; }
        public T Step { get; set; }
        public T Stop { get; set; }

        public IEnumerable<T> GenerateRange(Func<T,T,bool> condition, Func<T,T,T> updater) {
            for (var t = Start; condition(t, Stop); t = updater(t, Step)) yield return t;
        }
    }
}