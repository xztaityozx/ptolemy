using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Argo.Request {
    public class ArgoRequest {
        public Guid GroupId { get; set; }
        public string HspicePath { get; set; }
        public List<string> HspiceOptions { get; set; }
        public long Seed { get; set; }
        public long Sweep { get; set; }
        public long SweepStart { get; set; }
        public decimal Temperature { get; set; }
        public TransistorPair Transistors { get; set; }
        public RangeParameter Time { get; set; }
        public List<string> IcCommands { get; set; }
        public string NetList { get; set; }
        public List<string> Includes { get; set; }
        public decimal Vdd { get; set; }
        public decimal Gnd { get; set; }
        public List<string> Signals { get; set; }
        public string ResultFile { get; set; }
        public static ArgoRequest FromJson(string json) => JsonConvert.DeserializeObject<ArgoRequest>(json);
        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
