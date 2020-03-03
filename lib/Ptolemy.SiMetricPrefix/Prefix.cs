using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ptolemy.SiMetricPrefix
{
    /// <summary>
    /// Si接頭辞を数値に変換するクラス
    /// </summary>
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

        /// <summary>
        /// Decimalにパースする
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static decimal ParseDecimalWithSiPrefix(this string @this) => decimal.Parse(Replace(@this), NumberStyles.Float);
        /// <summary>
        /// Doubleにパースする
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static double ParseDoubleWithSiPrefix(this string @this) => double.Parse(Replace(@this), NumberStyles.Float);
        /// <summary>
        /// Intにパースする
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static int ParseIntWithSiPrefix(this string @this) => (int) @this.ParseDecimalWithSiPrefix();
        /// <summary>
        /// longにパースする
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static long ParseLongWithSiPrefix(this string @this) => (long) @this.ParseDecimalWithSiPrefix();

        /// <summary>
        /// Decimalに変換を試みる。成功したらTrueを返す
        /// </summary>
        /// <param name="this"></param>
        /// <param name="v">変換結果を受け取る変数</param>
        /// <returns></returns>
        public static bool TryParseDecimalWithSiPrefix(this string @this, out decimal v) {
            v = default;
            try {
                v = @this.ParseDecimalWithSiPrefix();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Doubleに変換を試みる。成功したらTrueを返す
        /// </summary>
        /// <param name="this"></param>
        /// <param name="v">変換結果を受け取る変数</param>
        /// <returns></returns>
        public static bool TryParseDoubleWithSiPrefix(this string @this, out double v) {
            v = default;
            try {
                v = @this.ParseDoubleWithSiPrefix();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
