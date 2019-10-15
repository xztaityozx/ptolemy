using System;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;
using Ptolemy.OptionException;

namespace Ptolemy.Aries {
    internal static class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();
            Console.Clear();
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            log.Warn("Press Ctrl+C to cancel");
            var token = cts.Token;

            try {
                Parser.Default.ParseArguments<AriesMake, AriesRun, AriesSearch>(args)
                    .MapResult(
                        (AriesMake a) => {
                            a.Run(token);
                            return 1;
                        },
                        (AriesRun a) => {
                            a.Run(token);
                            return 1;
                        },
                        (AriesSearch a) => {
                            a.Run();
                            return 1;
                        },
                        e => throw new ParseFailedException());
            }
            catch (ParseFailedException) {
            }
            catch (AriesException e) {
                log.Error(e);
            }
            catch (Exception e) {
                log.Error($"Unknown error has occured\n\t-->{e}");
            }
            
            Console.ResetColor();
        }
    }
}
