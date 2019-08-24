using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Lupus.Record;
using Ptolemy.Lupus.Repository;
using Ptolemy.Parameters;
using Ptolemy.PipeLine;
using ShellProgressBar;

namespace Ptolemy.Lupus
{
    [Verb("push", HelpText = "DataBaseにデータを書き込みます")]
    public class Push : Verb.Verb {

        [Option('d', "directory", HelpText = "Path to directory that contains csv files",Default = "")]
        public string Target { get; set; }

        [Option('f', "files", HelpText = "List of csv file's paths")]
        public IEnumerable<string> Files { get; set; }

        [Option('x',"parseParallel",Default = 1,HelpText = "パースの並列数です")]
        public int ParseParallel { get; set; }

        [Option('y', "pushParallel", Default = 1, HelpText = "DBへ書き込みタスクの並列数です")]
        public int PushParallel { get; set; }

        [Option('b',"buffer", Default = 50000, HelpText = "一度にDBへ書き込むレコードの数です")]
        public int QueueBuffer { get; set; }

        protected override Exception Do(CancellationToken token) {

            var request = string.IsNullOrEmpty(Target)
                ? new LupusPushRequest(Vtn, Vtp, Files)
                : new LupusPushRequest(Vtn, Vtp, Directory.EnumerateFiles(Target));

            //var opt = new ProgressBarOptions {
            //    ProgressCharacter = '=',
            //    ForegroundColor = ConsoleColor.DarkCyan
            //};

            //using (var p = new ProgressBar(5, "Master", new ProgressBarOptions {
            //    ProgressCharacter = '>',
            //    BackgroundCharacter = '-',
            //    ForegroundColor = ConsoleColor.DarkGreen,
            //    CollapseWhenFinished = false
            //})) {
                return PushToDatabase(token, request, null,null,null);
            //}
        }

        public Exception PushToDatabase(
            CancellationToken token,
            LupusPushRequest request,
            OnInnerIntervalEventHandler onParse,
            OnFinishEventHandler onFinish,
            Action onAllPushed
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
                    var res = pipeline.InitSelectMany(Files, ParseParallel, QueueBuffer, Factory.Build, null, onFinish,
                            null, onParse)
                        .Buffer(QueueBuffer, 100, null, onFinish).Out;

                    var status = pipeline.Start(() => {
                        foreach (var rs in res) {
                            repo.BulkUpsert(rs);
                        }

                        onAllPushed?.Invoke();
                        onFinish?.Invoke();
                    });

                    return status == PipeLine.PipeLine.PipeLineStatus.Completed ? null : new PipeLineException($"PipeLine status was {status}");
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
