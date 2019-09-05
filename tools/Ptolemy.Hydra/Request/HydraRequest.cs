using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Hydra.Server {
    public class HydraRequest {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public Range<long> Seed { get; set; }
        public bool UseDatabase { get; set; }
        public bool KeepCsv { get; set; }
        public Range<decimal> Sigma { get; set; }
        public long TotalSweeps { get; set; }
        public SweepSplitOption SweepSplitOption { get; set; }
        public List<string> Signals { get; set; }
        public Range<decimal> Time { get; set;}
        public Range<decimal> PlotPoint { get; set; }
        public string TargetCel { get; set; }
        public bool NotifyToSlackOnFinished { get; set; }
        public string SlackUserName { get; set; }
        public Guid Id { get; set; }
        public long SweepSplitSize { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static HydraRequest FromJson(string json) => JsonConvert.DeserializeObject<HydraRequest>(json);
        public static HydraRequest FromJson(StreamReader sr) => FromJson(sr.ReadToEnd());
    }

    public enum SweepSplitOption {
        NoSplit,
        SplitBySweep,
        SplitBySeed
    }

    public class Range<T> {
        public T Start { get; set; }
        public T Step { get; set; }
        public T Stop { get; set; }
    }
}