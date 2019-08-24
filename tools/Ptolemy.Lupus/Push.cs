using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Lupus.Record;
using ShellProgressBar;

namespace Ptolemy.Lupus
{
    [Verb("push")]
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
            return PushToDatabase(token, request);
        }

        public Exception PushToDatabase(CancellationToken token, LupusPushRequest request) {
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

                var opt = new ProgressBarOptions {
                    BackgroundCharacter = '-',
                    ProgressCharacter = '=',
                    ForegroundColor = ConsoleColor.DarkCyan
                };

                using (var parent = new ProgressBar(2, "Master", new ProgressBarOptions {
                    ProgressCharacter = '>',
                    BackgroundCharacter = '-',
                    CollapseWhenFinished = false
                }))
                using (var parse = parent.Spawn(request.FileList.Count, "parsing...", opt))
                using (var push = parent.Spawn(1, "pushing...", opt))
                using (var pipeline = new PipeLine.PipeLine(token)) {
                    var first = pipeline.InitSelectMany(Files, ParseParallel, QueueBuffer, Record.Factory.Build).Out;
                }

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
