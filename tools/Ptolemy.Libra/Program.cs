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

            Console.Clear();

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
                log.Info($"{request.Expressions.Count} expression(s) detected");
                log.Info($"Sweep");
                if (request.IsSplitWithSeed) {
                    log.Info($"\tSeed: Start: {request.SeedStart}, End: {request.SeedEnd}");
                    log.Info($"\tSweep per query: {request.Sweeps.Size}");
                    log.Info($"\tTotal Sweeps: {(request.SeedEnd - request.SeedStart + 1) * request.Sweeps.Size}");
                }
                else {
                    log.Info($"\tSeed: {request.SeedStart}");
                    log.Info($"\tSweep: Start: {request.Sweeps.Start}, End: {request.Sweeps.Total+request.Sweeps.Start-1}");
                }

                var libra = new Libra(token, log);
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
