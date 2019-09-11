using System;
using System.Diagnostics;
using System.Threading;
using CommandLine.Text;
using YamlDotNet.Core.Events;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            try {
                using (var cts = new CancellationTokenSource()) {
                    Console.CancelKeyPress += (sender, eventArgs) => {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    var opt = Options.Parse(args);
                    var argo = new Argo(opt, log);
                    argo.Run(cts.Token);
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
