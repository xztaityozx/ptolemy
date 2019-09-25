using System;
using System.Threading;
using CommandLine;
using Kurukuru;

namespace Ptolemy.Draco {

    /// <summary>
    /// Ptolemy.Dracoのエントリポイント
    /// </summary>
    internal static class Program {
        private static void Main(string[] args) {
            var logger = new Logger.Logger();
            try {
                // parse cli options
                var request = Parser.Default.ParseArguments<DracoOption>(args)
                    .MapResult(
                        o => o.Build(), e => throw new DracoParseFailedException());


                using var cts = new CancellationTokenSource();
                var token = cts.Token;
                Console.CancelKeyPress += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };
                logger.Warn("Press Ctrl+C to cancel");


                // start process
                Spinner.Start("Ptolemy.Draco ", spin => {
                    using var draco = new Draco(token, request);
                    // print logs to stdout
                    var d = draco.Log.Subscribe(s => logger.Info(s));
                    try {
                        draco.Run();
                        spin.Info("Completed");
                    }
                    catch (Exception) {
                        spin.Fail("Failed");
                    }
                    finally {
                        d.Dispose();
                    }
                });
            }
            catch (DracoException e) {
                logger.Error($"{e}");
            }
            catch (DracoParseFailedException) {
            }
            catch (Exception e) {
                logger.Fatal($"{e}");
            }
        }
    }
}