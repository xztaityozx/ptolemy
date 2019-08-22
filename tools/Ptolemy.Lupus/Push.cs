using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Lupus.Repository;
using Ptolemy.Parameters;
using ShellProgressBar;

namespace Ptolemy.Lupus {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : Verb.Verb {
        [Value(0, HelpText = "入力ファイルです", MetaName = "input")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('d', "directory", HelpText = "指定したディレクトリの下にあるファイルをすべてPushします")]
        public string TargetDirectory { get; set; }

        [Option('p', "Parallel", HelpText = "並列数です", Default = 1)]
        public int Parallel { get; set; }

        [Option('b', "bufferSize", Default = 50000, HelpText = "DBへのBulkUpsert一回当たりのEntityの個数です")]
        public int QueueBuffer { get; set; }

        private void PushFiles(CancellationToken token, ProgressBarBase parentBar) {

            var repo = new MssqlRepository();
            var name = Transistor.ToTableName(
                new Transistor(VtnThreshold, VtnSigma, VtnDeviation),
                new Transistor(VtpThreshold, VtpSigma, VtpDeviation)
            );
            repo.Use(name);

            var opt = new ProgressBarOptions {
                BackgroundCharacter = '-',
                ProgressCharacter = '>',
                ForegroundColor = ConsoleColor.DarkCyan
            };
            using (var pipeline = new PipeLine.PipeLine(token))
            using (var pushBar = parentBar.Spawn(1, "push...", opt))
            using (var parseBar = parentBar.Spawn(InputFiles.Count(), "parse...", opt)) {
                var first = pipeline.InitSelectMany(InputFiles, Parallel, QueueBuffer, Record.Factory.Build);
                first.OnInterval += (o) => parseBar.Tick($"parsed: {o}");
                first.OnFinish += () => parentBar.Tick();

                pipeline.Start(() => {
                    var list = new List<Record.Record>();
                    foreach (var r in first.Out) {
                        list.Add(r);
                        if (list.Count < QueueBuffer) continue;

                        // push to repository
                        repo.BulkUpsert(list);
                        list = new List<Record.Record>();
                    }

                    if (list.Any()) repo.BulkUpsert(list);
                });
                pushBar.Tick();
                parentBar.Tick();
            }
        }

        protected override Exception Do(CancellationToken token) {
            InputFiles = InputFiles.Any() ? InputFiles : Directory.EnumerateFiles(TargetDirectory);

            Logger.Info("Vtn:");
            Logger.Info($"\tThreshold: {VtnThreshold}");
            Logger.Info($"\tSigma: {VtnSigma}");
            Logger.Info($"\tDeviation: {VtnDeviation}");
            Logger.Info("Vtp:");
            Logger.Info($"\tThreshold: {VtpThreshold}");
            Logger.Info($"\tSigma: {VtpSigma}");
            Logger.Info($"\tDeviation: {VtpDeviation}");
            Logger.Info($"Total Files: {InputFiles.Count()}");

            Logger.Info("Start push");
            using (var bar = new ProgressBar(2, "Master", new ProgressBarOptions {
                ForegroundColor = ConsoleColor.DarkGreen,
                BackgroundCharacter = '-',
                ProgressCharacter = '>',
                CollapseWhenFinished = false
            })) {
                try {
                    PushFiles(token, bar);
                    return null;
                }
                catch (OperationCanceledException e) {
                    return e;
                }
                catch (Exception e) {
                    return new AggregateException(e);
                }
            }
        }
    }
}
