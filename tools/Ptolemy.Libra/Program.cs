using System;
using System.Reactive.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Libra.Request;
using Ptolemy.OptionException;

namespace Ptolemy.Libra {
    internal class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            try {
                Tuple<string, long>[] result = null;
                Spinner.Start("Ptolemy.Libra", spin => {
                    using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                        .Subscribe(s => spin.Text = $" {s}s")) {

                        log.Info("Start Ptolemy.Libra");
                        var request = Parser
                            .Default
                            .ParseArguments<LibraOption>(args)
                            .MapResult(o => o.BuildRequest(), e => throw new ParseFailedException());
                        log.Info("Built request");
                        var libra = new Libra(token, request);
                        result = libra.Run();
                        spin.Info("Finished aggregate");
                    }
                });

                foreach (var (key,value) in result) {
                    Console.WriteLine($"Expression: {key}, Value: {value}");
                }

            }
            catch (ParseFailedException) {
            }
            catch (LibraException e) {
                log.Error(e);
            }
            catch (Exception e) {
                log.Error($"Unknown has occured\n\t-->{e}");
            }
        }
    }
}
