using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Logger;
using Ptolemy.Map;
using Ptolemy.Repository;
using ShellProgressBar;

namespace Ptolemy.Aries {
    [Verb("run", HelpText = "シミュレーションを実行します")]
    public class AriesRun :IDisposable,IAriesVerb {
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

        private DbContainer GetDbContainer(CancellationToken token, IEnumerable<string> dbs, IObserver<string> sub) {
            log.Info("Build DbContainer...");
            log.Info("\tSearching databases...");
            return new DbContainer(token, DbRoot, dbs, BufferSize, sub);
        }

        private DbContainer container;
        public void Run(CancellationToken token) {
            log = new Logger.Logger();
            log.AddHook(new FileHook(Path.Combine(FilePath.FilePath.DotConfig, "log", "runLog")));
            
            var sw = new Stopwatch();
            sw.Start();
            try {
                MakeDbRoot();
                log.Info("Start Ptolemy.Aries run");
                Console.WriteLine();
                var requests = GetRequests();

                using var logSubject = new Subject<string>();
                logSubject.Subscribe(s => log.Info(s));
                container = GetDbContainer(token, requests.Select(s => s.ResultFile), logSubject);
                
                log.Info($"DbContainer has {container.Count} databases");
                log.Info($"Start simulation and write to db");
                Console.WriteLine();
                
                using var bar = new ProgressBar(requests.Count, "Ptolemy.Aries", new ProgressBarOptions {
                    BackgroundCharacter = '-', BackgroundColor = ConsoleColor.DarkGray,
                    ForegroundColor = ConsoleColor.DarkGreen, ProgressCharacter = '>',
                    CollapseWhenFinished = false, ForegroundColorDone = ConsoleColor.Green
                });

                //var receivers = new Map<string, Subject<ResultEntity>>();
                //foreach (var key in requests.Select(s=>s.ResultFile).Distinct()) {
                //    receivers[key] = new Subject<ResultEntity>();
                //    receivers[key].Subscribe(s => container[key].OnNext(s), token);
                //}

                foreach (var grouping in requests.GroupBy(s => s.ResultFile)) {
                    var db = grouping.Key;
                    var path = Path.Combine(FilePath.FilePath.DotConfig, "aries", "db", db + ".sqlite");

                    using var receiver = new Subject<ResultEntity>();

                    receiver.Buffer(BufferSize).Subscribe(r => {
                        using var repo = new SqliteRepository(path);
                        repo.BulkUpsert(r);
                    }, token);
                    grouping.AsParallel().WithCancellation(token).WithDegreeOfParallelism(Parallel)
                        .ForAll(req => {
                            using var argo = new Argo.Argo(req, token);
                            foreach (var resultEntity in argo.RunWithParse()) {
                                receiver.OnNext(resultEntity);
                            }
                        });
                }
                
                container.CloseAll();
            }
            catch (FileNotFoundException e) {
                log.Error($"{e.FileName} が見つかりませんでした");
            }
            
            Console.WriteLine();
            sw.Stop();
            log.Info("Finished Ptolemy.Aries run");
            log.Info($"Elapsed {sw.Elapsed}");
        }

        public void Dispose() {
            container?.Dispose();
        }
    }
}
