using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ptolemy.Repository {

    public class ParameterEntity {
        public long Id { get; set; }
        public string Vtn { get; set; }
        public string Vtp { get; set; }
        public string Includes { get; set; }
        public string Time { get; set; }
        public string IcCommand { get; set; }
        public decimal Gnd { get; set; }
        public decimal Vdd { get; set; }
        public string Signals { get; set; }
        public decimal Temperature { get; set; }
        public string HspiceOption { get; set; }
        public string NetList { get; set; }
        public string Hspice { get; set; }

        public ParameterEntity() { }

        public override string ToString() {
            var sb=new StringBuilder();

            sb.AppendLine($"Netlist: {NetList}");
            sb.AppendLine("Transistor");
            sb.AppendLine(
                $"  Vtn: (Threshold, Sigma, NumberOfSigma) {string.Join(", ", Vtn.Split('_').Where((_, i) => i % 2 == 1).Select(x => decimal.Parse(x, NumberStyles.Float)))}");
            sb.AppendLine(
                $"  Vtp: (Threshold, Sigma, NumberOfSigma) {string.Join(", ", Vtp.Split('_').Where((_, i) => i % 2 == 1).Select(x => decimal.Parse(x, NumberStyles.Float)))}");

            sb.AppendLine("Includes");
            foreach (var s in Includes.Split(':')) {
                sb.AppendLine($"  - {s}");
            }

            sb.AppendLine("Signals");
            foreach (var signal in Signals.Split(':')) {
                sb.AppendLine($"  - {signal}");
            }

            sb.AppendLine($"Simulation Time: {Time}");

            sb.AppendLine($"Temperature: {Temperature}");
            sb.AppendLine($"Voltage: \n  VDD: {Vdd}\n  GND: {Gnd}");
            sb.AppendLine($".IC");
            foreach (var s in IcCommand.Split(':')) {
                sb.AppendLine($"  - {s}");
            }

            sb.AppendLine("Hspice");
            sb.AppendLine($"  Path:    {Hspice}");
            sb.AppendLine($"  Options: {HspiceOption}");

            return sb.ToString();
        }

        public string Hash() {
            using var sha = SHA256.Create();
            return string.Join("", sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("", Vtn, Vtp, Includes, Time, IcCommand, $"{Gnd}{Vdd}{Temperature}", Signals, HspiceOption, NetList, Hspice))).Select(s => $"{s:X}"));
        }
    }
}
