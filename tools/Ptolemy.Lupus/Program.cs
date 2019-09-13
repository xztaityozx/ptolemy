using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;

namespace Ptolemy.Lupus {
    internal class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            try {
                log.Info("welcome to Ptolemy.Lupus");
                using (var cts = new CancellationTokenSource()) {
                    Console.CancelKeyPress += (sender, eventArgs) => {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    var sw = new Stopwatch();
                    sw.Start();
                    var request = Parser.Default.ParseArguments<Options>(args)
                        .MapResult(o => o.BuildLupusResult(), e => throw new LupusException(""));
                    var lupus = new Lupus(request);
                    cts.Token.ThrowIfCancellationRequested();
                    var result = lupus.Run(cts.Token);
                    sw.Stop();
                    if (result.Exception != null) {
                        log.Error("Failed Ptolemy.Lupus");
                        log.Error($"Last command: {result.ExecutedCommand}");
                        log.Error($"Error message: {result.Message}");
                    }
                    else {
                        log.Info("Finished Ptolemy.Lupus");
                        log.Info($"Elapsed time {sw.Elapsed}");
                    }
                }
            }
            catch (LupusException e) {
                log.Error($"[Ptolemy.Lupus] {e}");
                Environment.ExitCode = 1;
            }
            catch (OperationCanceledException) {
                log.Error("Canceled by user");
                Environment.ExitCode = 1;
            }
            catch (Exception e) {
                log.Error($"[Ptolemy.Lupus] unknown error has occured\n\t-->{e}");
                Environment.ExitCode = 1;
            }
        }
    }
}
