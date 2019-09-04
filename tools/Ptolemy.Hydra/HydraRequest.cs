using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using YamlDotNet.Serialization;

namespace Ptolemy.Hydra {
    public class HydraRequest  {
        [YamlMember(Alias = "id")] public Guid RequestId { get; set; }
        [YamlMember(Alias = "filename")] public string SelfPath { get; set; }
        [YamlMember(Alias = "result")] public string ResultFile { get; set; }
        [YamlMember(Alias = "dirs")] public HydraDirectories Directories { get; set; }
        [YamlMember(Alias = "parameters")] public HydraParameters Parameters { get; set; }

        public HydraRequest(){}
        public HydraRequest(string self, string result, HydraDirectories dirs, HydraParameters param) {
            RequestId=Guid.NewGuid();
            (SelfPath, ResultFile, Directories, Parameters) = (self, result, dirs, param);
        }

        public HydraRequest(string self, string baseDir, HydraParameters param) {
            SelfPath = self;
            Parameters = param;
            var local = Path.Combine(baseDir, $"{param.Vtn}_{param.Vtp}", $"{param.Seed}");
            Directories = new HydraDirectories {
                Simulation = Path.Combine(local, "sim"),
                Result = Path.Combine(local, "result"),
                NetList = Path.Combine(local, "netlist")
            };
        }

    }

    public class HydraDirectories : IEnumerable<string> {
        [YamlMember(Alias = "simDir")] public string Simulation { get; set; }
        [YamlMember(Alias = "netListDir")] public string NetList { get; set; }
        [YamlMember(Alias = "resultDir")] public string Result { get; set; }

        public HydraDirectories(){}
        public HydraDirectories(string s, string n, string r) => (Simulation, NetList, Result) = (s, n, r);

        public bool ExistAll() => new[] {Simulation, NetList, Result}.All(Directory.Exists);
        public IEnumerator<string> GetEnumerator() {
            foreach (var s in new[] {Simulation, NetList, Result}) yield return s;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class HydraParameters {
        [YamlMember(Alias = "vtn")] public Parameters.Transistor Vtn { get; set; }
        [YamlMember(Alias = "vtp")] public Parameters.Transistor Vtp { get; set; }
        [YamlMember(Alias = "seed")] public long Seed { get; set; }
        [YamlMember(Alias = "sweeps")] public long Sweeps { get; set; }
        [YamlMember(Alias = "model")] public string ModelFile { get; set; }
        [YamlMember(Alias = "vdd")] public decimal VddVoltage { get; set; }
        [YamlMember(Alias = "gnd")] public decimal GndVoltage { get; set; }
        [YamlMember(Alias = "icCommand")] public List<string> IcCommand { get; set; }
        [YamlMember(Alias = "CelDirectory")] public string CelDirectory { get; set; }
        [YamlMember(Alias = "sweepStart")] public long SweepStart { get; set; }
        [YamlMember(Alias = "time")] public TimeSpan Time { get; set; }
        [YamlMember(Alias = "plotPoint")] public TimeSpan PlotPoint { get; set; }
    }

    public class TimeSpan {
        [YamlMember] public decimal Start { get; set; }
        [YamlMember] public decimal Step { get; set; }
        [YamlMember] public decimal Stop { get; set; }
    }

}