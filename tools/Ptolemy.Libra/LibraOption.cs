using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Ptolemy.Interface;
using Ptolemy.Libra.Request;
using Ptolemy.SiMetricPrefix;
using Remotion.Linq.Parsing;

namespace Ptolemy.Libra {
    public class LibraOption {
        [Option('E', "expressions",Required = true, HelpText = "数え上げの条件式です。カンマ区切りです")]
        public string Expressions { get; set; }
        
        [Option('w', "sweep", Default = "1e7", HelpText = "合計Sweep数です")]
        public string SweepString { get; set; }
        
        [Option('e', "seed", HelpText = "Seedの値もしくは範囲([start],[end])を指定します", Default = "1")]
        public string SeedString { get; set; }

        [Option('W',"sweepStart", HelpText = "Sweepの初期値を指定します", Default = "1")]
        public string SweepStartString { get; set; }

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

            var seed = ParseSeedRequest(SeedString);

            if (string.IsNullOrEmpty(SweepStartString)) throw new NullReferenceException(nameof(SweepStartString));
            var sweepStart = SweepStartString.ParseLongWithSiPrefix();

            if (string.IsNullOrEmpty(SweepString)) throw new NullReferenceException(nameof(SweepString));
            return new LibraRequest(
                Expressions,  seed, (sweepStart, sweepStart + SweepString.ParseLongWithSiPrefix() - 1),
                SqliteFile);
        }


        private static (long start, long end) ParseSeedRequest(string request) {
            var split = request.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ParseLongWithSiPrefix())
                .ToList();
            return split switch {
                var x when x.Count == 2 => (split[0], split[1]),
                var x when x.Count == 1 => (split[0], split[0]),
                _ => throw new FormatException("Seedの指定がフォーマットに従っていません. [start],[end] or [value]")
                };
        }
    }
}