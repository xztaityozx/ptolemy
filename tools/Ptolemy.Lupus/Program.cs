using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Kurukuru;
using Microsoft.EntityFrameworkCore.Internal;
using Ptolemy.OptionException;

namespace Ptolemy.Lupus {
    internal class Program {
        private static void Main(string[] args) {


            Console.Clear();
            var log = new Logger.Logger();
            log.Info("Not Implemented Ptolemy.Lupus");
            return;
            try {
                Tuple<string, long>[] result = null;
                var sw = new Stopwatch();
                sw.Start();
                log.Info("Start Ptolemy.Lupus");
                Spinner.Start("Wait", spin => {
                    using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                        .Subscribe(l => { spin.Text = $" Elapsed: {l}s"; })) {
                        using var lupus = new Lupus(args, log);
                        result = lupus.Run<LupusException>();
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
                Console.WriteLine("\t" + Enumerable.Repeat("-", expWidth + valueWidth + 3).Join(""));

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
                log.Info("CleanUp tempDir...");
            }
        }
    }
}