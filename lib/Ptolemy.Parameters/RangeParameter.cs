using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Parameters {
    public class RangeParameter {
        public decimal Start { get; set; }
        public decimal Step { get; set; }
        public decimal Stop { get; set; }


        public RangeParameter() {
        }

        public RangeParameter((decimal start, decimal step, decimal stop) values) => (Start, Step, Stop) = values;

        public RangeParameter(decimal start, decimal step, decimal stop) : this((start, step, stop)) {
        }

        public RangeParameter(decimal start, decimal stop) : this(start, 1, stop) {
        }

        public RangeParameter(string value) {
            var split = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ParseDecimalWithSiPrefix()).ToArray();
            if (!split.Any()) throw new ArgumentException($"Invalid argument RangeParameter(string value <-- ): value={value}");
            switch (split.Length) {
                case 1:
                    Start = split[0];
                    Step = 1;
                    Stop = split[0];
                    break;
                case 2:
                    Start = split[0];
                    Step = split[0] > split[1] ? -1 : 1;
                    Stop = split[1];
                    break;
                default:
                    Start = split[0];
                    Step = split[1];
                    Stop = split[2];
                    break;
            }
        }

        public RangeParameter(string value, (decimal start, decimal step, decimal stop) def) {
            var split = value.Split(',').Zip(new[] {def.start, def.step, def.stop},
                (s, d) => string.IsNullOrEmpty(s) ? d : s.ParseDecimalWithSiPrefix()).ToArray();
            switch (split.Length) {
                case 1:
                    Start = split[0];
                    Step = 1;
                    Stop = split[0];
                    break;
                case 2:
                    Start = split[0];
                    Step = split[0] > split[1] ? -1 : 1;
                    Stop = split[1];
                    break;
                default:
                    Start = split[0];
                    Step = split[1];
                    Stop = split[2];
                    break;
            }
        }

        public IEnumerable<decimal> ToEnumerable() {
            decimal a = (decimal) Start, b = (decimal) Step, c = (decimal) Stop;
            for (var d = a; d <= c; d += b) yield return d;
        }
    }
}