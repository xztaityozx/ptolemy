using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;
using Ptolemy.Map;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Libra.Request
{
    using FilterFunc = Func<Map.Map<string,decimal>, bool>;
    public class LibraRequest {
        public Dictionary<string,string> Conditions { get; set; }
        public List<string> Expressions { get; set; }
        public string SqliteFile { get; set; }

        public List<FilterFunc> BuildFilter() {
            var rt = new List<FilterFunc>();
            var map = new Map<string, string>();
            foreach (var (key,value) in Conditions) {
                map[key] = Parse(value);
            }

            {
                var itr = new Interpreter();
                foreach (var expression in Expressions) {
                    var expr = string.Join("", expressionOperators
                        .Aggregate(expression, (exp, op) => exp.Replace(op, $" {op} "))
                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => map.ContainsKey(s) ? "(" + map[s] + ")" : s));


                    rt.Add(itr.ParseAsDelegate<FilterFunc>(expr, "map"));
                }
            }

            return rt;
        }


        private readonly string[] operators = {"<=", ">=", "==", "!=", "<", ">"};
        private readonly string[] expressionOperators = {"&&", "||", "(", ")", "!"};

        private string Parse(string condition) {
            foreach (var op in operators) {
                var split = condition.Split(op, StringSplitOptions.RemoveEmptyEntries);
                if(split.Length!=2) continue;

                var a = split[0];
                var b = split[1];
                return $"{Value(a)}{op}{Value(b)}";
            }

            throw new ArgumentException($"Bad condition string: {condition}", nameof(condition));
        }

        private static string Value(string value) {
            var split = value.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 1) return $"{value.ParseDecimalWithSiPrefix()}M";


            var signal = split[0].Trim(' ');
            var time = split[1].ParseDecimalWithSiPrefix();
            return $"map[\"{GetKey(signal, time)}\"]";
        }

        public static string GetKey(string signal, decimal time) => $"{signal}/{time:E5}";
    }

}
