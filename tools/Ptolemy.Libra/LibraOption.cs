using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Ptolemy.Interface;
using Ptolemy.Libra.Request;

namespace Ptolemy.Libra {
    public class LibraOption: IRangeOption {
        [Option('E', "expressions",Required = true, HelpText = "数え上げの条件式です。カンマ区切りです")]
        public string Expressions { get; set; }

        [Value(0, Required = true, HelpText = "ターゲットのSQLiteファイルです", MetaName = "targetDB")]
        public string SqliteFile { get; set; }


        public LibraRequest BuildRequest() {

            if(string.IsNullOrEmpty(Expressions)) throw new LibraException("Expressionsが空です");

            SqliteFile = FilePath.FilePath.Expand(SqliteFile);

            if (!File.Exists(SqliteFile)) {
                throw new LibraException($"SQLiteファイル {SqliteFile} が見つかりません");
            }

            var (w, e) = this.Bind((null, null));
            var rt = new LibraRequest(
                Expressions, ((long) w.Start, (long) w.Stop), ((long) e.Start, (long) e.Stop),
                SqliteFile);

            return rt;
        }

        public string Sweep { get; set; }
        public string Seed { get; set; }
    }
}