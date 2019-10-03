using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.OptionException;

namespace Ptolemy.Lupus {
    internal class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };
            var token = cts.Token;

            try {
                Tuple<string, long>[] result = null;
                var sw = new Stopwatch();
                sw.Start();
                Spinner.Start("Wait", spin => {
                    using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                        .Subscribe(l => spin.Text = $" Elapsed: {l}s")) {
                        log.Info("Start Ptolemy.Lupus");
                        var req = Parser.Default.ParseArguments<LupusOptions>(args)
                            .MapResult(o => o.BuildRequests(), e => throw new ParseFailedException());

                        var lupus = new Lupus();
                        result = lupus.Run(token, req);
                        spin.Info("Finished");
                    }
                });
                sw.Stop();
                log.Info($"Finished Ptolemy.Lupus");
                log.Info($"Elapsed time: {sw.Elapsed}");
                Console.WriteLine();

                foreach (var (exp, val) in result) {
                    Console.WriteLine($"Expression: {exp}, Value: {val}");
                }
            }
            catch (ParseFailedException) {
            }
            catch (OperationCanceledException) {
                log.Error("Canceled by user");
            }
            catch (LupusException le) {
                log.Error(le);
            }
            catch (Exception e) {
                log.Error($"Unknown error has occured\n\t-->{e}");
            }
        }
    }
}
