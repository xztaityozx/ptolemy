using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ptolemy.Parameters
{
    public class Range {
        public decimal Start { get; set; }
        public decimal Step { get; set; }
        public decimal Stop { get; set; }


        public Range() {}
        public Range((decimal start, decimal step, decimal stop) values) => (Start, Step, Stop) = values;
        public Range(decimal start, decimal step, decimal stop) : this((start, step, stop)) { }

        public IEnumerable<decimal> ToEnumerable() {
            for (var d = Start; d <= Stop; d += Step) yield return d;
        }
    }

}
