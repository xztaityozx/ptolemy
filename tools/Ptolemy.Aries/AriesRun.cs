﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Argo.Request;
using Ptolemy.Logger;
using Ptolemy.Map;
using Ptolemy.Repository;
using Ptolemy.SiMetricPrefix;
using Ptolemy.Simulator;
using ShellProgressBar;

namespace Ptolemy.Aries {
    [Verb("run", HelpText = "シミュレーションを実行します")]
    public class AriesRun : IDisposable, IAriesVerb {
        [Option("parallel", Default = 1, HelpText = "シミュレーションの並列数です")]
        public int Parallel { get; set; }

        [Option('n', "count", HelpText = "実行するタスクの数です", Default = "1")]
        public string Count { get; set; }

        [Option("all", Default = false, HelpText = "保存されているタスクをすべて実行します")]
        public bool All { get; set; }

        [Option('i', "input", HelpText = "タスクファイルを指定して実行します", Default = null)]
        public string InputFile { get; set; }

        [Option('y', "yes", HelpText = "確認をスキップします", Default = false)]
        public bool Yes { get; set; }

        [Option('b', "bufferSize", HelpText = "1度のDbアクセスで書き込むアイテムの最大数です", Default = 50000)]
        public int BufferSize { get; set; }

        [Option('m', "maxRetry", Default = 3, HelpText = "ひとつのシミュレーションが失敗したときに再実行する回数の上限値です")]
        public int MaxRetry { get; set; }

        [Option("slack", Default = false, HelpText = "シミュレーションの開始時と終了時にSlackへ投稿します")]
        public bool PostToSlack { get; set; }

        private Logger.Logger log;

        private string dbRoot, taskDir, logDir;

        /// <summary>
        /// DBを保存するディレクトリを作る
        /// </summary>
        private void MakeDbRoot() {
            dbRoot = FilePath.FilePath.Expand(Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "db"));
            if (Directory.Exists(dbRoot)) return;
            log.Warn($"DbRoot: {dbRoot} not found");

            bool Ask() {
                Console.Write($"Dbファイルを {dbRoot} に保存しますか？(y/n)>> ");
                return Console.ReadLine() switch {"y" => true, "yes" => true, _ => false};
            }

            if (Yes || Ask()) {
                Directory.CreateDirectory(dbRoot);
            }
            else {
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Requestのリストを返す
        /// </summary>
        /// <returns></returns>
        private List<(ArgoRequest request, string filePath)> GetRequests() {
            var tasks = new List<(ArgoRequest, string)>();
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
                tasks.Add((ArgoRequest.FromJson(doc), InputFile));
            }
            else {
                taskDir = FilePath.FilePath.Expand(Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "task"));
                if (!Directory.Exists(taskDir)) throw new AriesException($"{taskDir} が見つかりません");

                tasks.AddRange(Directory.GetFiles(taskDir)
                    .TakeWhile((s, i) => i < count || All)
                    .Select(s => (ArgoRequest.FromFile(s), s)));
            }

            log.Info($"Total task = {tasks.Count}");
            if (!tasks.Any()) {
                throw new AriesException("タスクが0個でした。終了します");
            }
            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dbs"></param>
        /// <param name="sub"></param>
        /// <returns></returns>
        private AriesDbContainer GetDbContainer(CancellationToken token, IEnumerable<ParameterEntity> dbs, IObserver<string> sub) {
            log.Info("Build AriesDbContainer...");
            log.Info("\tSearching databases...");
            AriesDbContainer rt = null;
            Spinner.Start("Building AriesDbContainer...", () => rt = new AriesDbContainer(token, dbRoot, dbs, BufferSize, sub));
            return rt;
        }

        /// <summary>
        /// シミュレーションをする。削除していいタスクファイルへのパスのリストを返す
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requests"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<string> StartSimulation(CancellationToken token, IEnumerable<(ArgoRequest, string)> requests,
            AriesRunProgressBar parent) {
            var rt = new List<string>();
            requests
                .OrderBy(_ => Guid.NewGuid()) // DbContainerは同じDBへ並列書き込みしないので、できるだけ書き込み先のDBをばらけさせる
                .AsParallel().WithDegreeOfParallelism(Parallel) // requestを並列化
                .WithCancellation(token) // キャンセル可能
                .ForAll(req => {
                        var (request, filePath) = req;
                        using var bar = parent.SpawnSimBar((int) request.Sweep,
                            $"Sweep: {request.Sweep}, Seed:{request.Seed}, Transistor: {request.Transistors}");

                        var hspice = new Hspice();
                        var retry = 0;

                        // リトライ機構
                        do {
                            try {
                                foreach (var r in hspice.Run(token, request, () => bar.Tick())) {
                                    container[request.ResultFile].OnNext(r);
                                }
                                log.Info($"Finished {filePath}");
                                break;
                            }
                            catch (Exception e) {
                                retry++;
                                log.Error(e);
                                log.Warn($"task {filePath} will retry({retry})...");
                            }
                        } while (retry < MaxRetry);

                        // リトライ数の上限に達してないので削除対象にする
                        if (retry < MaxRetry) {
                            rt.Add(filePath);
                        }
                        else {
                            log.Error($"Retry数の上限に達しました: task file -> {filePath}");
                        }

                        // Progress
                        parent.TickSimBar();
                    }
                );
            parent.SetTextToWriteBar("Closing AriesDbContainer...");
            container.CloseAll();
            return rt;
        }

        private AriesDbContainer container;

        
        public void Run(CancellationToken token) {
            log = new Logger.Logger();
            {
                logDir = FilePath.FilePath.Expand(Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "log"));
                FilePath.FilePath.TryMakeDirectory(logDir);
                var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
                log.AddHook(new FileHook(logFile));
                log.Info("Add file hook");
                log.Info($"log file: {logFile}");
            }


            if (PostToSlack) {
                var b = Slack.Slack.ReplyTo($"Ptolemy.Aries run: Started at {DateTime.Now}",
                    Config.Config.Instance.SlackConfig);
                if(b) log.Info("Posted to Slack");
                else log.Error("Failed post to Slack");
            }

            var sw = new Stopwatch();
            var deleteTarget = new List<string>();
            var (success, failed) = (0, 0);
            sw.Start();
            try {
                MakeDbRoot();
                log.Info("Start Ptolemy.Aries run");
                Console.WriteLine();
                var requests = GetRequests();

                // log出力用Subject
                using var logSubject = new Subject<string>();
                logSubject.Subscribe(s => log.Info(s));


                container = GetDbContainer(token,
                    // ParameterEntityにしてHashでDistinctする
                    requests.Select(s => SimulatorExtension.ConvertToParameterEntity(s.request))
                        .Distinct(item => item.Hash()),
                    logSubject);

                log.Info($"AriesDbContainer has {container.Count} databases");
                log.Info($"Start simulation and write to db");
                Console.WriteLine();


                var totalRecords = 0;

                try {
                    totalRecords = (int) requests.Select(s =>s.request.ExpectedRecords).Sum();
                }
                catch (OverflowException) {
                    throw new AriesException($"一度に処理できるレコードの数が{int.MaxValue}を超えました。タスクの数を調整することを検討してください");
                }

                log.Info($"Ptolemy.Aries will generate {totalRecords} records");

                using var parent =
                    new AriesRunProgressBar(requests.Count, totalRecords);
                logSubject.Subscribe(s => parent.TickWriteBar(int.Parse(s)));

                deleteTarget = StartSimulation(token, requests, parent);
                success = deleteTarget.Count;
                failed = requests.Count - success;
            }
            catch (FileNotFoundException e) {
                log.Error($"{e.FileName} が見つかりませんでした");
            }

            Console.WriteLine();

            Spinner.Start("Cleaning up...", spin => {
                foreach (var item in deleteTarget) {
                    File.Delete(item);
                    spin.Text = $"{item} removing...";
                }

                spin.Info("Finished");
            });
            sw.Stop();
            log.Info("Finished Ptolemy.Aries run");
            log.Info($"Elapsed {sw.Elapsed}");
            if (!PostToSlack) return;
            if(Slack.Slack.PostToAriesResult(success, failed, sw.Elapsed, Config.Config.Instance.SlackConfig)) log.Info("Posted to Slack");
            else log.Error("Failed post to Slack");
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

        public AriesRunProgressBar(int totalRequests, int totalRecords) {
            parent = new ProgressBar(2, "Ptolemy.Aries run", new ProgressBarOptions {
                BackgroundCharacter = '-', ProgressCharacter = '>',
                BackgroundColor = ConsoleColor.DarkGray, ForegroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = false, DisplayTimeInRealTime = true,
                ForegroundColorDone = ConsoleColor.Green
            });

            sim = parent.Spawn(totalRequests, "Simulation", second);
            write = parent.Spawn(totalRecords, "Write to database", second);
        }

        public IProgressBar SpawnSimBar(int totalSweep, string msg = "") {
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

    internal static class DistinctExt {
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> selector) {

            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var map = new Map<TKey, bool>(false);
            foreach (var item in @this) {
                var key = selector(item);
                if(map[key]) continue;
                map[key] = true;
                yield return item;
            }
        }
    }
}
