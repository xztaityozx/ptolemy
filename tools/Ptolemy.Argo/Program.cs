using System;
using System.Diagnostics;
using System.Threading;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            try {
                using (var cts = new CancellationTokenSource()) {
                    log.Info("[Ptolemy.Argo] Welcome to Ptolemy.Argo CLI");
                    log.Warn("Press Ctrl+C to cancel");

                    Console.CancelKeyPress += (sender, eventArgs) => {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    var opt = Options.Parse(args);
                    var argo = new Argo(opt, log);
                    var sw = new Stopwatch();
                    sw.Start();
                    var res = argo.Run(cts.Token);
                    sw.Stop();

                    log.Info($"Elapsed time {sw.Elapsed}");
                    log.Info($"Simulation result files were output to {res.ResultDir}");

                }
            }
            catch (ArgoParseFailedException e) {
                log.Error($"Failed parse command line options\n\tinnerException-->{e}");
                Environment.ExitCode = 1;
            }
            catch (ArgoException e) {
                log.Error($"[Ptolemy.Argo] {e}");
                Environment.ExitCode = 1;
            }
            catch (Exception e) {
                log.Error($"[Ptolemy.Argo] Unknown error has occured\n\t-->{e}");
                Environment.ExitCode = 1;
            }
        }
    }

    public class ArgoParseFailedException:Exception {}
}
