using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            log.Warn("Press Ctrl+C to cancel");

            try {
                Tuple<string, long>[] result = null;
                var sw = new Stopwatch();
                sw.Start();

                log.Info("Start Ptolemy.Libra");
                var request = Parser
                    .Default
                    .ParseArguments<LibraOption>(args)
                    .MapResult(o => o.BuildRequest(), e => throw new ParseFailedException());
                log.Info("Built request");
                var libra = new Libra(token);
                result = libra.Run(request);

                sw.Stop();
                log.Info($"Elapsed time: {sw.Elapsed}");
                log.Info("Result: ");
                Console.WriteLine();
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
