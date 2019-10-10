using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using ShellProgressBar;

namespace Ptolemy.Aries {
    [Verb("run", HelpText = "シミュレーションを実行します")]
    public class AriesRun :IDisposable {
        [Option("parallel", Default = 1, HelpText = "シミュレーションの並列数です")]
        public int Parallel { get; set; }

        [Option('n', "count", HelpText = "実行するタスクの数です", Default = 1)]
        public int Count { get; set; }

        [Option("taskDir", Default = "~/.config/ptolemy/aries/task", HelpText = "タスクが保存されているディレクトリへのパスです")]
        public string TaskDir { get; set; }

        [Option('R',"dbRoot", HelpText = "DBファイルの格納されるディレクトリルートへのパスです", Default = "~/.config/ptolemy/aries/dbRoot")]
        public string DbRoot { get; set; }

        [Option("all", Default = false, HelpText = "保存されているタスクをすべて実行します")]
        public bool All { get; set; }

        [Option('i', "input", HelpText = "タスクファイルを指定して実行します", Default = null)]
        public string InputFile { get; set; }

        [Option('y', "yes",  HelpText = "確認をスキップします", Default = false)] 
        public bool Yes { get; set; }
        [Option('b', "bufferSize", HelpText = "1度のDbアクセスで書き込むアイテムの最大数です", Default = 50000)]
        public int BufferSize { get; set; }

        private Logger.Logger log;
        private void MakeDbRoot() {
            DbRoot = FilePath.FilePath.Expand(DbRoot);
            if (Directory.Exists(DbRoot)) return;
            log.Warn($"DbRoot: {DbRoot} not found");

            bool Ask() {
                Console.Write($"Dbファイルを {DbRoot} に保存しますか？(y/n)>> ");
                return Console.ReadLine() switch { "y" => true, "yes" => true, _ => false };
            }

            if (Yes || Ask()) {
                Directory.CreateDirectory(DbRoot);
            }
            else {
                throw new OperationCanceledException();
            }
        }

        private List<ArgoRequest> GetRequests() {
            var tasks = new List<ArgoRequest>();

            if (Count <= 0) throw new AriesException("Countを0以下にできません");

            if (!string.IsNullOrEmpty(InputFile)) {
                string doc;
                using (var sr = new StreamReader(InputFile)) doc = sr.ReadToEnd();
                tasks.Add(ArgoRequest.FromJson(doc));
            }
            else {
                TaskDir = FilePath.FilePath.Expand(TaskDir);
                if (!Directory.Exists(TaskDir)) throw new AriesException($"{TaskDir} が見つかりません");

                tasks.AddRange(Directory.GetFiles(TaskDir)
                    .TakeWhile((s, i) => i < Count || All)
                    .Select(ArgoRequest.FromFile));
            }

            log.Info($"Total task = {tasks.Count}");
            return tasks;
        }

        private DbContainer GetDbContainer(CancellationToken token, IEnumerable<string> dbs) {
            using var sub = new Subject<string>();
            using (sub.Subscribe(s => log.Info(s)))
                return new DbContainer(token, DbRoot, dbs, BufferSize, sub);
        }

        private DbContainer container;
        private ProgressBar bar;
        public void Run(CancellationToken token) {
            log = new Logger.Logger();

            try {
                MakeDbRoot();
                log.Info("Ptolemy.Aries run");
                var requests = GetRequests();
                container = GetDbContainer(token, requests.Select(s => s.ResultFile));
                bar = new ProgressBar(requests.Count, "Ptolemy.Aries", ConsoleColor.DarkGreen);

                requests
                    .AsParallel()
                    .WithCancellation(token)
                    .WithDegreeOfParallelism(Parallel)
                    .ForAll(req => {
                        var db = req.ResultFile;
                        var rec = new Subject<ResultEntity>();
                        rec.Subscribe(s => container.Add(db, s), () => bar.Tick(), token);
                        using var argo = new Argo.Argo(req, token);
                        argo.RunWithParse(rec);
                    });
            }
            catch (FileNotFoundException e) {
                log.Error($"{e.FileName} が見つかりませんでした");
            }
        }

        public void Dispose() {
            container?.Dispose();
            bar?.Dispose();
        }
    }
}
