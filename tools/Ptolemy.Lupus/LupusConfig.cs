using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Ptolemy.Map;
using DynamicExpresso;
using Ptolemy.SiMetricPrefix;
using YamlDotNet.Serialization;

namespace Ptolemy.Lupus {
    using FilterFunc = Func<Map<string, decimal>, bool>;

    public class LupusConfig {

        // conditions BNF
        // <condition> := "<name>": "<cond>"
        // <cond> := <value><operator><value>
        // <value> := <signal>, <number>
        // <signal> := <signalName>[<time>]
        // <time> := <number>
        // <number> := [0-9](.[0-9]+)*, [0-9](.[0-9]+)<siUnit> 
        // <siUnit> := G, M, K, m, u, n, p
        // <operator> := <, <=, >, >=, !=, ==
        // <signalName> := <string>
        // <name> := <string>
        // <string> := ([a-zA-Z0-9])+
        [YamlMember(Alias = "conditions")]
        public Dictionary<string, string> Conditions { get; set; }
        [YamlMember(Alias = "expressions")]
        public List<string> Expressions { get; set; }
        [YamlMember(Alias = "logDir")]
        public string LogDir { get; set; }
        [YamlMember(Alias = "ConnectionString")]
        public string ConnectionString { get; set; }

        public LupusConfig() { }

        private static LupusConfig config;
        public static LupusConfig Instance {
            get {
                if (config != null) return config;

                // parse from json or yml
                var yml = Path.Combine(FilePath.FilePath.DotConfig, "lupus.yml");
                var json = Path.Combine(FilePath.FilePath.DotConfig, "lupus.json");

                var path = File.Exists(yml) ? yml :
                    File.Exists(json) ? json :
                    throw new FileNotFoundException(
                        $"There is no lupus.yml or lupus.json under {FilePath.FilePath.DotConfig}"
                    );

                try {
                    string str;
                    using (var sr = new StreamReader(path)) str = sr.ReadToEnd();
                    config = new Deserializer().Deserialize<LupusConfig>(str);
                }
                catch (Exception e) {
                    throw new AggregateException("Failed to parse config file", e);
                }

                config.LogDir = FilePath.FilePath.Expand(config.LogDir);

                return config;
            }
        }
    }

    public class FilterDelegates {
        public readonly Func<Map<string, decimal>, bool> Filter;
        public readonly string Name;

        public FilterDelegates(string name, FilterFunc filter) {
            Filter = filter;
            Name = name;
        }
    }

    public class Filter {
        public IReadOnlyList<FilterDelegates> Delegates { get; }

        public Filter(Dictionary<string, string> conditions, IEnumerable<string> expressions) {
            var map = new Map<string, string>();
            foreach (var (key, value) in conditions) {
                map[key] = Decode(value);
            }

            filterList.AddRange(conditions.Select(x => $"{x.Key}: {x.Value}"));
            var ops = new[] {"||", "&&", "(", ")", "!"};
            var itr = new Interpreter();

            var ds = (from item in expressions
                let exp = string.Join("", ops.Aggregate(item, (cur, op) => cur.Replace(op, $" {op} "))
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => map.ContainsKey(s) ? map[s] : s))
                select new FilterDelegates(item, itr.ParseAsDelegate<FilterFunc>(exp, "map"))).ToList();

            Delegates = ds;
        }

        private readonly string[] operators = {"<", ">", "<=", ">=", "==", "!="};

        private string Decode(string cond) {
            var box = operators.Skip(2).Aggregate(cond, (current, op) => current.Replace(op, $" {op} "))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (box.Length != 3) {
                box = operators.Take(2).Aggregate(cond, (current, op) => current.Replace(op, $" {op} "))
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
            }

            return $"{Value(box[0])}{box[1]}{Value(box[2])}";
        }

        public Tuple<string, long>[] Aggregate(List<Map<string, decimal>> list) {
            var map = new Map<string, long>();

            foreach (var d in Delegates) {
                map[d.Name] = list.Count(d.Filter);
            }

            return map.Select(x => Tuple.Create(x.Key, x.Value)).ToArray();
        }

        private readonly List<string> filterList = new List<string>();

        public override string ToString() {
            return string.Join("\n", filterList);
        }

        private static string Value(string value) {
            return value.TryParseDecimalWithSiPrefix(out var x) ? $"{x}M" : Signal(value);
        }

        private static string Signal(string value) {
            var box = value.Split(new[] {"[", "]"}, StringSplitOptions.RemoveEmptyEntries);
            var signal = box[0];
            var time = box[1].ParseDecimalWithSiPrefix();
            return $"map[\"{Record.Record.EncodeKey(signal, time)}\"]";
        }
    }
}
