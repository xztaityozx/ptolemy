using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Lupus.Record;
using Ptolemy.Lupus.Repository;
using Ptolemy.Parameters;
using Ptolemy.PipeLine;
using ShellProgressBar;

namespace Ptolemy.Lupus {
    [Verb("push", HelpText = "DataBaseにデータを書き込みます")]
    public class Push : Verb.Verb {

        [Option('d', "directory", HelpText = "Path to directory that contains csv files", Default = "")]
        public string Target { get; set; }

        [Option('f', "files", HelpText = "List of csv file's paths")]
        public IEnumerable<string> Files { get; set; }

        [Option('x', "parseParallel", Default = 1, HelpText = "パースの並列数です")]
        public int ParseParallel { get; set; }

        [Option('y', "pushParallel", Default = 1, HelpText = "DBへ書き込みタスクの並列数です")]
        public int PushParallel { get; set; }

        [Option('b', "buffer", Default = 50000, HelpText = "一度にDBへ書き込むレコードの数です")]
        public int QueueBuffer { get; set; }

        protected override Exception Do(CancellationToken token) {

            var request = string.IsNullOrEmpty(Target)
                ? new LupusPushRequest(Vtn, Vtp, Files)
                : new LupusPushRequest(Vtn, Vtp, Directory.EnumerateFiles(Target));

            Exception res = null;
            Spinner.Start("Pushing...", spin => {
                res = PushToDatabase(token, request, spin);
                if (res == null) spin.Succeed("Finished all pipeline");
                else spin.Fail("some problem has occured");
            });
            return res;
        }

        public Exception PushToDatabase(
            CancellationToken token,
            LupusPushRequest request,
            Spinner spin
        ) {
            try {
                token.ThrowIfCancellationRequested();

                Logger.Info("Vtn:");
                Logger.Info($"\tVoltage: {VtnThreshold}");
                Logger.Info($"\tSigma: {VtnSigma}");
                Logger.Info($"\tDeviation: {VtnDeviation}");
                Logger.Info("Vtp:");
                Logger.Info($"\tVoltage: {VtpThreshold}");
                Logger.Info($"\tSigma: {VtpSigma}");
                Logger.Info($"\tDeviation: {VtpDeviation}");
                Logger.Info($"Total Files: {request.FileList.Count}");


                var repo = new MssqlRepository();
                repo.Use(Transistor.ToTableName(Vtn, Vtp));

                using (var pipeline = new PipeLine.PipeLine(token)) {
                    var result = pipeline.InitSelectMany(
                            request.FileList, ParseParallel, QueueBuffer, Factory.Build, () => {
                                Console.WriteLine();
                                Logger.Info("Begin: parsing");
                            },
                            () => {
                                Console.WriteLine();
                                Logger.Info("Finished: Parsing");
                            },
                            s => spin.Text = $"Parsed: {s}"
                        ).Buffer(QueueBuffer, 100, () => {
                            Console.WriteLine();
                            Logger.Info("Begin: buffering");
                        }, () => {
                            Console.WriteLine();
                            Logger.Info("Finished: Buffering");
                        })
                        .Then(PushParallel, QueueBuffer, rs => repo.BulkUpsert(rs), () => {
                            Console.WriteLine();
                            Logger.Info("Begin: pushing");
                        }, () => {
                            Console.WriteLine();
                            Logger.Info("Finished: pushing");
                        }).Out;

                    pipeline.Start(() => { });

                    var innerExceptions = result as Exception[] ?? result.ToArray();
                    return innerExceptions.Any() ? new AggregateException(innerExceptions) : null;
                }

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