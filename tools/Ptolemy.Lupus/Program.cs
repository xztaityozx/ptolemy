using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using Microsoft.EntityFrameworkCore.Internal;
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
                var req = Parser.Default.ParseArguments<LupusOptions>(args)
                    .MapResult(o => o.BuildRequests(), e => throw new ParseFailedException());

                Spinner.Start("Wait", spin => {
                    using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                        .Subscribe(l => spin.Text = $" Elapsed: {l}s")) {
                        log.Info("Start Ptolemy.Lupus");
                        var lupus = new Lupus();
                        result = lupus.Run(token, req);
                        spin.Info("Finished");
                    }
                });
                sw.Stop();
                log.Info($"Finished Ptolemy.Lupus");
                log.Info($"Elapsed time: {sw.Elapsed}");
                Console.WriteLine("========================================================");
                Console.WriteLine();

                var expWidth = Math.Max(result.Select(s => s.Item1.Length).Max(), " Expressions ".Length);
                var valueWidth = Math.Max(result.Select(s => $"{s.Item2}".Length).Max(), " Value ".Length);

                Console.WriteLine($"\t|{{0,{expWidth}}}|{{1, {valueWidth}}}|", " Expressions ", " Value ");
                Console.WriteLine("\t"+Enumerable.Repeat("-", expWidth+valueWidth+3).Join(""));

                foreach (var (exp, val) in result) {
                    Console.WriteLine($"\t|{{0,{expWidth}}}|{{1,{valueWidth}}}|", exp, val);
                }

                Console.WriteLine();
                Console.WriteLine("========================================================");

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
            finally {
                var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
                if (Directory.Exists(tmp)) {
                    log.Info("CleanUp tempDir...");
                    Directory.Delete(tmp, true);
                }
            }
        }
    }
}
