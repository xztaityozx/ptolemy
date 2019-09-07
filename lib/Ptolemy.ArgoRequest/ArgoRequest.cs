using System;
using System.Collections.Generic;
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
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public Range Time { get; set; }
        public List<string> IcCommands { get; set; }
        public string TargetCircuit { get; set; }
        public string BaseDirectory { get; set; }
        public string ModelFilePath { get; set; }
        public decimal Vdd { get; set; }
        public decimal Gnd { get; set; }
    }
}
