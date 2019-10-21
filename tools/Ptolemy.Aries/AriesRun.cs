using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Argo;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using Ptolemy.SiMetricPrefix;
using ShellProgressBar;

namespace Ptolemy.Aries {
    [Verb("run", HelpText = "シミュレーションを実行します")]
    public class AriesRun :IDisposable,IAriesVerb {
        [Option("parallel", Default = 1, HelpText = "シミュレーションの並列数です")]
        public int Parallel { get; set; }

        [Option('n', "count", HelpText = "実行するタスクの数です", Default = "1")]
        public string Count { get; set; }

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
            var count = 1;
            try {
                count = Count.ParseIntWithSiPrefix();
            }
            catch (Exception) {
                log.Error($"{Count} をパースできませんでした");
                Environment.Exit(1);
            }

            if (count <= 0) throw new AriesException("Countを0以下にできません");

            if (!string.IsNullOrEmpty(InputFile)) {
                string doc;
                using (var sr = new StreamReader(InputFile)) doc = sr.ReadToEnd();
                tasks.Add(ArgoRequest.FromJson(doc));
            }
            else {
                TaskDir = FilePath.FilePath.Expand(TaskDir);
                if (!Directory.Exists(TaskDir)) throw new AriesException($"{TaskDir} が見つかりません");

                tasks.AddRange(Directory.GetFiles(TaskDir)
                    .TakeWhile((s, i) => i < count || All)
                    .Select(ArgoRequest.FromFile));
            }

            log.Info($"Total task = {tasks.Count}");
            return tasks;
        }

        private DbContainer GetDbContainer(CancellationToken token, IEnumerable<string> dbs, IObserver<string> sub) {
            log.Info("Build DbContainer...");
            log.Info("\tSearching databases...");
            DbContainer rt = null;
            Spinner.Start("Building DbContainer...", () => rt = new DbContainer(token, DbRoot, dbs, BufferSize, sub));
            return rt;
        }

        private DbContainer container;
        public void Run(CancellationToken token) {
            log = new Logger.Logger();
            
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

                var totalRecords = requests.Select(s => (int) s.Sweep * s.Signals.Count * s.Time.ToEnumerable().Count()).Sum();
                log.Info($"Ptolemy.Aries will generate {totalRecords} records");

                using var parent =
                    new AriesRunProgressBar(requests.Count, totalRecords);
                logSubject.Subscribe(s => parent.TickWriteBar(int.Parse(s)));

                requests
                    .OrderBy(_ => Guid.NewGuid())                   // DbContainerは同じDBへ並列書き込みしないので、できるだけ書き込み先のDBをばらけさせる
                    .AsParallel().WithDegreeOfParallelism(Parallel) // requestを並列化
                    .WithCancellation(token)                        // キャンセル可能
                    .ForAll(req => {
                            using var bar = parent.SpawnSimBar((int) req.Sweep,
                                $"Sweep: {req.Sweep}, Seed:{req.Seed}, Transistor: {req.Transistors}");

                            var hspice = new Hspice();
                            foreach (var r in hspice.Run(token, req, bar)) {
                                container[req.ResultFile].OnNext(r);
                            }

                            parent.TickSimBar();
                        }
                    );
                parent.SetTextToWriteBar("Closing DbContainer...");
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

    internal class AriesRunProgressBar : IDisposable {
        private readonly ProgressBar parent;
        private readonly IProgressBar sim, write;

        private readonly ProgressBarOptions second = new ProgressBarOptions {
                BackgroundCharacter = '-', 
                ProgressCharacter = '=',
                BackgroundColor = ConsoleColor.Gray, 
                ForegroundColor = ConsoleColor.DarkYellow,
            },
            inner = new ProgressBarOptions {
                BackgroundCharacter = '_',
                ProgressCharacter = '#',
                BackgroundColor = ConsoleColor.Gray,
                ForegroundColor = ConsoleColor.DarkCyan,
            };

        public AriesRunProgressBar(int totalRequests,int totalRecords) {
            parent=new ProgressBar(2, "Ptolemy.Aries run", new ProgressBarOptions {
                BackgroundCharacter = '-', ProgressCharacter = '>',
                BackgroundColor = ConsoleColor.DarkGray, ForegroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = false, DisplayTimeInRealTime = true,
                ForegroundColorDone = ConsoleColor.Green
            });

            sim = parent.Spawn(totalRequests, "Simulation", second);
            write = parent.Spawn(totalRecords, "Write to database", second);
        }

        public IProgressBar SpawnSimBar(int totalSweep, string msg="") {
            return sim.Spawn(totalSweep, msg, inner);
        }

        public void TickSimBar(string msg = "") {
            sim.Tick(msg);

            if (sim.MaxTicks == sim.CurrentTick) parent.Tick("Finished simulation");
        }

        public void SetTextToWriteBar(string msg = "") => write.Message = msg;

        public void TickWriteBar(int size) {
            var cnt = Math.Min(size, write.MaxTicks - write.CurrentTick);
            for (var i = 0; i < cnt; i++) {
                write.Tick();
            }

            if (write.MaxTicks == write.CurrentTick) parent.Tick("Finished write to database");
        }

        public void Dispose() {
            parent?.Dispose();
            sim?.Dispose();
            write?.Dispose();
        }
    }
}
