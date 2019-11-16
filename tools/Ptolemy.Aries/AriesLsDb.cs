using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Interface;
using Ptolemy.Parameters;
using Ptolemy.Repository;

namespace Ptolemy.Aries {
    [Verb("ls-db", HelpText = "DBをとそのパラメータをリストアップします")]
    public class AriesLsDb :IAriesVerb{
        [Option('d',"dir",HelpText = "DBが保存されているディレクトリへのパスです")]
        public string Dir { get; set; }

        private readonly string baseDir = Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "db");

        public void Run(CancellationToken token) {
            Dir = string.IsNullOrEmpty(Dir)
                ? baseDir
                : FilePath.FilePath.Expand(Dir);

            if(!Directory.Exists(Dir)) throw new DirectoryNotFoundException(Dir);

            var dbs = Directory.GetFiles(Dir, "*.sqlite");
            if (!dbs.Any()) {
                throw new AriesException($"{Dir}以下にDbがありませんでした。");
            }
            foreach (var db in dbs) {
                token.ThrowIfCancellationRequested();

                var repo = new ReadOnlyRepository(Path.Combine(baseDir, db));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Path: {db}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(repo.GetParameter());

                Console.WriteLine("---------------------------------------------------------------------");
            }
        }
    }
}