using System;
using System.Collections.Generic;

namespace Ptolemy.Lupus.Entity {
    public class Entity {
        public Guid Id { get; set; }
        public long Sweep { get; set; }
        public long Seed { get; set; }
        public Dictionary<string, decimal> Values { get; set; }

        public Entity() {}

        public Entity(long sweep, long seed, Record.Record[] records) {
            Sweep = sweep;
            Seed = seed;
            Values = new Dictionary<string, decimal>();
            foreach (var record in records) {
                Values.Add(record.Key, record.Value);
            }
        }
    }
}