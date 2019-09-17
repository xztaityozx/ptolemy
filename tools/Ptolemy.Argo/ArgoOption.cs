using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy.Argo {
    public class ArgoOption : IPtolemyTransistorOption {
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }

        public override string ToString() {
            return $"Vtn:{string.Join(",", VtnStrings)}, Vtp:{string.Join(",",VtpStrings)}, Sigma?:{Sigma}";
        }
    }
}
