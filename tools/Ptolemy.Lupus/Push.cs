using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Lupus.Record;
using Ptolemy.Lupus.Repository;
using Ptolemy.Parameters;
using Ptolemy.PipeLine;
using ShellProgressBar;
using System.Reactive.Linq;
using System.Reactive.Threading;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Remotion.Linq.Clauses.Expressions;

namespace Ptolemy.Lupus {
    [Verb("push", HelpText = "DataBaseにデータを書き込みます")]
    public class Push : Verb.Verb {

        [Option('d', "directory", HelpText = "Path to directory that contains csv files", Default = "")]
        public string Target { get; set; }

        [Option('f', "files", HelpText = "List of csv file's paths")]
        public IEnumerable<string> Files { get; set; }

        [Option('b', "buffer", Default = 50000, HelpText = "一度にDBへ書き込むレコードの数です")]
        public int QueueBuffer { get; set; }

        protected override Exception Do(CancellationToken token) {

            var request = string.IsNullOrEmpty(Target)
                ? new LupusPushRequest(Vtn, Vtp, Files)
                : new LupusPushRequest(Vtn, Vtp, Directory.EnumerateFiles(Target));

            Logger.Info("Vtn:");
            Logger.Info($"\tVoltage: {request.Vtn.Threshold}");
            Logger.Info($"\tSigma: {request.Vtn.Sigma}");
            Logger.Info($"\tDeviation: {request.Vtn.Deviation}");
            Logger.Info("Vtp:");
            Logger.Info($"\tVoltage: {request.Vtp.Threshold}");
            Logger.Info($"\tSigma: {request.Vtp.Sigma}");
            Logger.Info($"\tDeviation: {request.Vtp.Deviation}");
            Logger.Info($"Total Files: {request.FileList.Count}");
            Logger.Info($"DatabaseName: {Transistor.ToTableName(Vtn, Vtp)}");

            return PushToDatabase(token, request); 
        }

        public Exception PushToDatabase(
            CancellationToken token,
            LupusPushRequest request
        ) {
            try {
                token.ThrowIfCancellationRequested();
                var repo = new MssqlRepository();
                repo.Use(Transistor.ToTableName(Vtn, Vtp));

                using (var parent = new ProgressBar(2, "Master", ConsoleColor.DarkBlue)) {
                    IList<Record.Record>[] container = null;
                    Spinner.Start("Parsing...", () =>
                        container = request.FileList.ToObservable().SelectMany(Factory.Build).Buffer(QueueBuffer)
                            .ToEnumerable().ToArray());
                    parent.Tick("Finished Parsing...");

                    using (var push = parent.Spawn(container.Length, "Pushing...")) {
                        var cnt = 0;
                        foreach (var records in container ?? throw new NullReferenceException())
                        {
                            token.ThrowIfCancellationRequested();

                            using (var sub = push.Spawn(100, "sub")) repo.BulkUpsert (token, records, sub);
                            push.Tick($"{cnt += records.Count} records was pushed");
                        }
                    }
                    parent.Tick("Finished Pushing...");
                }
                Logger.Info($"Finished push {request.FileList.Count} files");

                return null;
            }
            catch (OperationCanceledException e) {
                return new OperationCanceledException($"Canceled by User\n\t--> {e}");
            }
            catch (Exception e) {
                return e;
            }
        }
    }
}