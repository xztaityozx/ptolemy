using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Ptolemy.Interface;
using Ptolemy.Libra.Request;

namespace Ptolemy.Libra {
    public class LibraOption {
        [Option('E', "expressions",Required = true, HelpText = "数え上げの条件式です。カンマ区切りです")]
        public string Expressions { get; set; }
        
        [Option('w', "sweep", Default = "1e7", HelpText = "合計Sweep数です")]
        public string SweepString { get; set; }
        
        [Option('W', "sweepSplitOption", Default = "sweep", HelpText = "合計Sweepを分割する方法を指定します。sweepを指定するとseedを固定します。seedを指定するとseedを固定します")]
        public string SweepSplitOption { get; set; }

        [Value(0, Required = true, HelpText = "ターゲットのSQLiteファイルです", MetaName = "targetDB")]
        public string SqliteFile { get; set; }


        public LibraRequest BuildRequest() {

            if(string.IsNullOrEmpty(Expressions)) throw new LibraException("Expressionsが空です");

            SqliteFile = FilePath.FilePath.Expand(SqliteFile);

            if (!File.Exists(SqliteFile)) {
                throw new LibraException($"SQLiteファイル {SqliteFile} が見つかりません");
            }
            
            

            var rt = new LibraRequest(
                Expressions, ((long) w.Start, (long) w.Stop), ((long) e.Start, (long) e.Stop),
                SqliteFile);

            return rt;
        }
    }
}