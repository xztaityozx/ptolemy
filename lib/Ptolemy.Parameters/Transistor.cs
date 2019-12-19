using System;
using System.Collections.Generic;

namespace Ptolemy.Parameters
{
    public class TransistorPair {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }

        public TransistorPair() { }
        public TransistorPair(Transistor vtn, Transistor vtp) => (Vtn, Vtp) = (vtn, vtp);
        public TransistorPair((decimal t, decimal s, decimal d) vtn, (decimal t, decimal s, decimal d) vtp)
            : this(new Transistor(vtn), new Transistor(vtp)) { }
        public TransistorPair((double t, double s, double d) vtn, (double t, double s, double d) vtp)
            : this(new Transistor(vtn.t, vtn.s, vtn.d), new Transistor(vtp.t, vtp.s, vtp.d)) { }
        public TransistorPair(double vtnT, double vtnS, double vtnD, double vtpT, double vtpS, double vtpD)
            : this((vtnT, vtnS, vtnD), (vtpT, vtpS, vtpD)) { }
        public TransistorPair(decimal vtnT, decimal vtnS, decimal vtnD, decimal vtpT, decimal vtpS, decimal vtpD)
            : this((vtnT, vtnS, vtnD), (vtpT, vtpS, vtpD)) { }

        public override string ToString() {
            return $"Vtn_{Vtn}_Vtp_{Vtp}";
        }
    }


    public class Transistor {
        public decimal? Threshold { get; set; }
        public decimal? Sigma { get; set; }
        public decimal? NumberOfSigma { get; set; }

        public Transistor() { }
        public Transistor(decimal t, decimal s, decimal d) => (Threshold, Sigma, NumberOfSigma) = (t, s, d);

        public Transistor(double t, double s, double d) : this((decimal) t, (decimal) s, (decimal) d) {}

        public Transistor((decimal, decimal, decimal) value) : this(value.Item1, value.Item2, value.Item3) { }

        public Transistor(string str) {
            var split = str.Split(',');
            if(split.Length<3) throw new ArgumentException($"Invalid argument: {str} <--");

            Threshold = SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix(split[0]);
            Sigma = SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix(split[1]);
            NumberOfSigma = SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix(split[2]);
        }

        /// <summary>
        /// Get parameter's string
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetParameterStrings() {
            yield return $"{nameof(Threshold)}: {Threshold}";
            yield return $"{nameof(Sigma)}: {Sigma}";
            yield return $"{nameof(NumberOfSigma)}: {NumberOfSigma}";
        }

        public override string ToString() {
            return $"t_{Threshold:E5}_s_{Sigma:E5}_d_{NumberOfSigma:E5}";
        }

    }
}
