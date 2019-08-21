using System.Collections.Generic;
using System.Globalization;

namespace Ptolemy.SiMetricPrefix
{
    public static class SiMetricPrefix {
        private static readonly Dictionary<string, string> PrefixTable = new Dictionary<string, string> {
            ["Y"] = "e24",
            ["Z"] = "e21",
            ["E"] = "e18",
            ["P"] = "e15",
            ["T"] = "e12",
            ["G"] = "e9",
            ["M"] = "e6",
            ["K"] = "e3",
            ["h"] = "e2",
            ["da"] = "e1",
            ["d"] = "e-1",
            ["c"] = "e-2",
            ["m"] = "e-3",
            ["u"] = "e-6",
            ["n"] = "e-9",
            ["p"] = "e-12",
            ["f"] = "e-15",
            ["a"] = "e-18",
            ["z"] = "e-21",
            ["y"] = "e-24"
        };

        private static string Replace(string str) {
            foreach (var (k, v) in PrefixTable) {
                str = str.Replace(k, v);
            }

            return str;
        }

        public static decimal ParseDecimal(this string @this) => decimal.Parse(Replace(@this), NumberStyles.Float);
        public static double ParseDouble(this string @this) => double.Parse(Replace(@this), NumberStyles.Float);
        public static int ParseInt(this string @this) => (int) @this.ParseDecimal();
        public static long ParseLong(this string @this) => (long) @this.ParseDecimal();
    }
}
