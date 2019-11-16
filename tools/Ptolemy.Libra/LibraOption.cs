using System;
using System.IO;
using System.Linq;
using CommandLine;
using Ptolemy.Libra.Request;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Libra {
    public class LibraOption {
        [Option('E', "expressions",Required = true, HelpText = "数え上げの条件式です。カンマ区切りです")]
        public string Expressions { get; set; }
        
        [Option('w', "sweep", Default = "2000x5000", HelpText = "合計Sweep数です。Sweepの分割する場合は[個数]x[sweep]")]
        public string Sweep { get; set; }
        
        [Option('e', "seed", HelpText = "Seedの値もしくは範囲([start],[end])を指定します", Default = "1")]
        public string Seed { get; set; }

        [Option('W',"sweepStart", HelpText = "Sweepの初期値を指定します", Default = "1")]
        public string SweepStart { get; set; }

        [Value(0, Required = true, HelpText = "ターゲットのSQLiteファイルです", MetaName = "targetDB")]
        public string SqliteFile { get; set; }


        public LibraRequest BuildRequest() {

            if(string.IsNullOrEmpty(Expressions)) throw new LibraException("Expressionsが空です");

            try {
                SqliteFile = FilePath.FilePath.Expand(SqliteFile);
            }
            catch (NullReferenceException) {
                throw new LibraException($"SQLiteへのパスが空です");
            }

            if (!File.Exists(SqliteFile)) {
                throw new LibraException($"SQLiteファイル {SqliteFile} が見つかりません");
            }

            var seed = ParseRangeRequest(Seed, ',');

            if (string.IsNullOrEmpty(SweepStart)) throw new NullReferenceException(nameof(SweepStart));

            if (string.IsNullOrEmpty(Sweep)) throw new NullReferenceException(nameof(Sweep));
            return new LibraRequest(
                Expressions,  seed, Sweep, SweepStart.ParseLongWithSiPrefix(), SqliteFile);
        }


        private static (long first, long second) ParseRangeRequest(string request, char delimiter) {
            var split = request.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ParseLongWithSiPrefix())
                .ToList();
            return split switch {
                var x when x.Count == 2 => (split[0], split[1]),
                var x when x.Count == 1 => (split[0], split[0]),
                _ => throw new FormatException($"{nameof(request)}の指定がフォーマットに従っていません. [start],[end] or [value]")
                };
        }
    }
}