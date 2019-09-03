using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Ptolemy.Hydra {
    public class HydraRequest {
        [YamlMember(Alias = "id")]
        public Guid RequestId { get; set; }
        [YamlMember(Alias = "filename")]
        public string SelfPath { get; set; }
        [YamlMember(Alias = "result")]
        public string ResultFile { get; set; }
        [YamlMember(Alias = "dirs")]
        public HydraDirectories Directories { get; set; }

        public HydraRequest(){}
        public HydraRequest(string self, string result, HydraDirectories dirs) {
            RequestId=Guid.NewGuid();
            (SelfPath, ResultFile, Directories) = (self, result, dirs);
        }
    }

    public class HydraDirectories {
        [YamlMember(Alias = "simDir")]
        public string Simulation { get; set; }
        [YamlMember(Alias = "netListDir")]
        public string NetList { get; set; }
        [YamlMember(Alias = "resultDir")]
        public string Result { get; set; }

        public HydraDirectories(){}
        public HydraDirectories(string s, string n, string r) => (Simulation, NetList, Result) = (s, n, r);

        public bool ExistAll() => new[] {Simulation, NetList, Result}.All(Directory.Exists);
    }

    public class HydraParameters {
        [YamlMember(Alias = "vtn")]
        public Parameters.Transistor Vtn { get; set; }
        [YamlMember(Alias = "vtp")]
        public Parameters.Transistor Vtp { get; set; }
        [YamlMember(Alias = "seed")]
        public long Seed { get; set; }
        [YamlMember(Alias = "sweeps")]
        public long Sweeps { get; set; }
        [YamlMember(Alias = "model")]
        public string ModelFile { get; set; }
        [YamlMember(Alias = "vdd")]
        public decimal VddVoltage { get; set; }
        [YamlMember(Alias = "gnd")]
        public decimal GndVoltage { get; set; }
        [YamlMember(Alias = "icCommand")]
        public List<string> IcCommand { get; set; }
        [YamlMember(Alias = "netListFile")]
        public string NetListFile { get; set; }
        [YamlMember(Alias = "sweepStart")]
        public long SweepStart { get; set; }
    }
}