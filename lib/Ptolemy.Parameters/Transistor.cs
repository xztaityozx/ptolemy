using System.Collections.Generic;

namespace Ptolemy.Parameters {
    public class Transistor {
        public decimal Threshold { get; set; }
        public decimal Sigma { get; set; }
        public decimal Deviation { get; set; }

        public Transistor(decimal t, decimal s, decimal d) => (Threshold, Sigma, Deviation) = (t, s, d);

        public Transistor(double t, double s, double d) : this((decimal) t, (decimal) s, (decimal) d) {}

        public Transistor((decimal, decimal, decimal) value) : this(value.Item1, value.Item2, value.Item3) { }

        /// <summary>
        /// Get parameter's string
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetParameterStrings() {
            yield return $"{nameof(Threshold)}: {Threshold}";
            yield return $"{nameof(Sigma)}: {Sigma}";
            yield return $"{nameof(Deviation)}: {Deviation}";
        }

        public override string ToString() {
            return $"t_{Threshold:E10}_s_{Sigma:E10}_d_{Deviation:E10}";
        }

        public static string ToTableName(Transistor vtn, Transistor vtp) =>
            $"vtn_{vtn}_vtp_{vtp}";
    }
}
