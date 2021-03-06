﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;
using Ptolemy.Map;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Libra.Request
{
    using FilterFunc = Func<Map.Map<string,decimal>, bool>;

    /// <summary>
    /// Libraの実行計画
    /// </summary>
    public class LibraRequest {
        /// <summary>
        /// [値][比較演算子][値]というシンプルな条件式のリスト
        /// </summary>
        public Dictionary<string,string> Conditions { get; set; }
        /// <summary>
        /// Conditionsに格納した条件式を使った数え上げ条件式のリスト
        /// </summary>
        public List<string> Expressions { get; set; }
        public List<string> ExpressionNameList { get; }
        public string SqliteFile { get; set; }
        public long SeedStart { get; set; }
        public long SeedEnd { get; set; }
        public Sweeps Sweeps { get; set; }


        private List<string> signals = new List<string>();
        public IReadOnlyList<string> SignalList => signals;
        private List<decimal> times = new List<decimal>();
        public IReadOnlyList<decimal> TimeList => times;

        public bool IsSplitWithSeed => SeedStart != SeedEnd;

        public LibraRequest() { }

        /// <summary>
        /// ConditionsとExpressionsからWhereに渡すデリゲートを生成する
        /// </summary>
        /// <returns></returns>
        public List<FilterFunc> BuildFilter() {
            var rt = new List<FilterFunc>();
            var map = new Map<string, string>();
            foreach (var (key,value) in Conditions) {
                map[key] = Parse(value);
            }

            {
                var itr = new Interpreter();
                foreach (var expression in Expressions) {
                    var delegateString = string.Join("", expressionOperators
                        .Aggregate(expression, (exp, op) => exp.Replace(op, $" {op} "))
                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => map.ContainsKey(s) ? "(" + map[s] + ")" : s));


                    rt.Add(itr.ParseAsDelegate<FilterFunc>(delegateString, "map"));
                }
            }

            times = times.Distinct().ToList();
            signals = signals.Distinct().ToList();

            return rt;
        }

        public LibraRequest(string expressionString, (long start, long end) seed, string sweepRequest, long sweepStart, string sqliteFile) {
            ExpressionNameList = new List<string>(expressionString.Split(',', StringSplitOptions.RemoveEmptyEntries));
            var expressions = expressionString.Replace(" ", "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                    expressionOperators.Aggregate(s, (exp, op) => exp.Replace(op, $" {op} ")).Replace(" ! =", "!="))
                .ToList();

            Conditions = new Dictionary<string, string>();
            foreach (var item in expressions.SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !expressionOperators.Contains(s))
                .Distinct().Select((s, i) => new {s, i = i + 1})) {
                Conditions[$"exp{item.i}"] = item.s;
            }

            Expressions = new List<string>();
            foreach (var expression in expressions) {
                Expressions.Add(Conditions.Aggregate(expression, (s, kv) => s.Replace(kv.Value, kv.Key)));
            }

            Sweeps = new Sweeps(sweepRequest, sweepStart);
            (SeedStart, SeedEnd) = seed;
            SqliteFile = sqliteFile;
        }

        private readonly string[] operators = {"<=", ">=", "==", "!=", "<", ">"};
        private readonly string[] expressionOperators = {"&&", "||", "(", ")", "!"};

        private string Parse(string condition) {
            // <condition> ::= <value><operator><value>
            // <value>  ::= <float> | <variable> 
            // <float> ::= <decimal> | <decimal><si prefix>
            // <decimal> ::= 10進数
            // <si prefix> ::= Y ~ yで表される 10^24 ~ 10^-24 のSi接頭辞
            // <variable> ::= <string>[<float>]
            // <string> ::= スペースを含まない文字列
            // <operator> ::= <= | >= | == | != | < | >

            foreach (var op in operators) {
                // <operator> で split して要素が2つになるものを探す
                var split = condition.Split(op, StringSplitOptions.RemoveEmptyEntries);
                if(split.Length!=2) continue;

                var a = split[0];
                var b = split[1];
                return $"{Value(a)}{op}{Value(b)}";
            }

            throw new LibraException($"Bad condition string: {condition}");
        }

        private string Value(string value) {
            try {
                var split = value.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
                if (!split.Any()) throw new Exception();

                if (split.Length == 1) return $"{value.ParseDecimalWithSiPrefix()}M";

                var signal = split[0].Trim(' ');
                signals.Add(signal);
                var time = split[1].ParseDecimalWithSiPrefix();
                times.Add(time);
                return $"map[\"{GetKey(signal, time)}\"]";
            }
            catch (Exception) {
                throw new LibraException($"パースできませんでした. 問題個所 --> {value}");
            }
        }

        /// <summary>
        /// Signal/Time という文字列を作る
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetKey(string signal, decimal time) => $"{signal}/{time:E5}";
    }

    public class LibraException : Exception {
        public LibraException(string msg) : base(msg) {
        }
    }
}
