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
            Logger.Info($"\tVoltage: {VtnThreshold}");
            Logger.Info($"\tSigma: {VtnSigma}");
            Logger.Info($"\tDeviation: {VtnDeviation}");
            Logger.Info("Vtp:");
            Logger.Info($"\tVoltage: {VtpThreshold}");
            Logger.Info($"\tSigma: {VtpSigma}");
            Logger.Info($"\tDeviation: {VtpDeviation}");
            Logger.Info($"Total Files: {request.FileList.Count}");
            Logger.Info($"DatabaseName: {Transistor.ToTableName(Vtn, Vtp)}");

            Exception res = null;
            Spinner.Start("Pushing to database...", spin => {
                res = PushToDatabase(token, request);
                if (res == null) spin.Succeed("Finished");
                else spin.Fail("some problem has occured");
            });
            return res;
        }

        public Exception PushToDatabase(
            CancellationToken token,
            LupusPushRequest request
        ) {
            try {
                token.ThrowIfCancellationRequested();
                var repo = new MssqlRepository();
                repo.Use(Transistor.ToTableName(Vtn, Vtp));

                repo.BulkUpsertRange(request.FileList.ToObservable()
                    .SelectMany(Factory.Build)
                    .Buffer(QueueBuffer).ToEnumerable().ToList());

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