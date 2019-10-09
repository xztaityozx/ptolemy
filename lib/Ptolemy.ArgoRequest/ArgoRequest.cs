using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public static ArgoRequest FromFile(string path) {
            using var sr = new StreamReader(path);
            return FromJson(sr.ReadToEnd());
        }

        public string GetHashString() {
            using var sha256 = SHA256.Create();

            return string.Join("", sha256.ComputeHash(
                Encoding.UTF8.GetBytes(
                    string.Join("", new[] {$"{Transistors}", $"{Gnd}", $"{Vdd}", $"{Temperature}", NetList}
                        .Concat(IcCommands)
                        .Concat(Includes)))
            ).Select(s => $"{s:X2}"));
        }
    }
}
