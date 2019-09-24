using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Ptolemy.Map;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Repository {
    public class ResultEntity {
        [Required] public decimal Time { get; set; }
        [Required] public decimal Value { get; set; }
        [Required] public string Signal { get; set; }
        [Required] public long Seed { get; set; }
        [Required] public long Sweep { get; set; }
        
        public ResultEntity(){}

        public static IEnumerable<ResultEntity> Parse(
            long seed, long sweep, string input,
            IEnumerable<string> keys) {
            var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var time = split[0].ParseDecimalWithSiPrefix();

            return split.Skip(1)
                .Select(s => s.ParseDecimalWithSiPrefix())
                .Zip(keys, (d, s) => new {s, d})
                .Select(value => new ResultEntity {
                    Seed = seed,
                    Sweep = sweep,
                    Signal = value.s,
                    Time = time,
                    Value = value.d
                });
        }
    }
}