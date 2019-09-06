using System;
using System.Collections.Generic;
using Ptolemy.Parameters;

namespace Ptolemy.ArgoRequest {
    public class ArgoRequest {
        public long Seed { get; set; }
        public long Sweep { get; set; }
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
