using System;
using System.Collections.Generic;
using System.Linq;

namespace Ptolemy.Libra.Request {
    public class Sweeps {
        public long Start { get; set; }
        public long Total => Size * Times;
        /// <summary>
        /// Sweep size per simulation
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Number of simulations
        /// </summary>
        public long Times { get; set; }

        public Sweeps() { }

        public Sweeps(string str, long start) {
            Start = start;
            var split = str.Split('x', StringSplitOptions.RemoveEmptyEntries).Select(SiMetricPrefix.SiMetricPrefix.ParseLongWithSiPrefix).ToList();
            (Times, Size) = split switch {
                var x when x.Count == 2 => (split[0], split[1]),
                var x when x.Count == 1 => (1, split[0]),
                _ => throw new FormatException($"入力のフォーマットが正しくありません. {str}")
                };
        }

        /// <summary>
        /// Generate sweep list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> Repeat() {
            for (var l = 0L; l < Times; l++) yield return Size;
        }

        /// <summary>
        /// Generate sweep section list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(long start, long end)> Section() {
            var s = Start;
            for (var i = 0; i < Times; i++) {
                yield return (s, s + Size - 1);
                s += Size;
            }
        }
    }
}