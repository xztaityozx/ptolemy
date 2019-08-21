namespace Ptolemy.Lupus.Record {
    public class Record {
        public long Sweep { get; set; }
        public decimal Value { get; set; }
        public long Seed { get; set; }
        public string Key { get; set; }

        public Record() {
        }

        public Record(long sweep, long seed, string signal, decimal time, decimal value) {
            Sweep = sweep;
            Seed = seed;
            Key = EncodeKey(signal, time);
            Value = value;
        }

        public override string ToString() {
            return $"Signal/Time:{Key}, Sweep:{Sweep}, Value:{Value}, Seed:{Seed}";
        }

        public static string EncodeKey(string signal, decimal time) => $"{signal}/{time:E10}";
    }
}