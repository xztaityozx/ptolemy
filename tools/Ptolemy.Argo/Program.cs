using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.OptionException;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {


            var log = new Logger.Logger();
            log.Info("Welcome to Ptolemy.Argo");
            try {
                var req = Parser.Default.ParseArguments<ArgoOption>(args)
                    .MapResult(o => {
                        if (!o.Clean) return o.BuildRequest();
                        var path = Path.Combine(Path.GetTempPath(), "Ptolemy.Argo");
                        Spinner.Start("Cleanup...", spin => {
                            if (Directory.Exists(path)) Directory.Delete(path, true);
                            spin.Info("Finished");
                        });


                        throw new ArgoClean();
                    }, e => throw new ParseFailedException());

                Console.Clear();
                log.Info($"TargetNetList Netlist: {req.NetList}");
                log.Info("Parameters");
                log.Info("\tVtn:");
                log.Info($"\t\tThreshold: {req.Transistors.Vtn.Threshold}");
                log.Info($"\t\tSigma: {req.Transistors.Vtn.Sigma}");
                log.Info($"\t\tDeviation: {req.Transistors.Vtn.Deviation}");
                log.Info("\tVtp:");
                log.Info($"\t\tThreshold: {req.Transistors.Vtp.Threshold}");
                log.Info($"\t\tSigma: {req.Transistors.Vtp.Sigma}");
                log.Info($"\t\tDeviation: {req.Transistors.Vtp.Deviation}");
                log.Info($"\tSweeps: {req.SweepStart:E} to {req.Sweep + req.SweepStart - 1:E}");
                log.Info($"\tSeed: {req.Seed:E}");
                log.Info($"\tTemperature: {req.Temperature}");
                log.Info($"\tSimulationTime: {req.Time.Start:E} to {req.Time.Stop:E} (step={req.Time.Step:E})");
                log.Info($"ExtractTargets: {string.Join(",", req.Signals)}");
                log.Info($"Include {req.Includes.Count} files");
                foreach (var inc in req.Includes) {
                    log.Info($"\t--> {inc}");
                }

                log.Warn($"Press Ctrl+C to cancel");

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };

                var sw = new Stopwatch();
                sw.Start();
                var res = Argo.Run(cts.Token, req);
                sw.Stop();

                log.Info("Finished simulation");
                log.Info($"Elapsed: {sw.Elapsed}");
                log.Info($"{res.Count} result records");
                if (string.IsNullOrEmpty(req.ResultFile)) {
                    log.Warn("result file not set. print to stdout");
                    Console.WriteLine(JsonSerializer.Serialize(res));
                }
                else {
                    log.Info($"Write to {req.ResultFile}");
                    using var writer = new StreamWriter(req.ResultFile);
                    writer.WriteLine(JsonSerializer.Serialize(res));
                    writer.Flush();
                }
            }
            catch (ArgoClean) {
                log.Info("Ptolemy.Argo clean temp directory /tmp/Ptolemy.Argo");
            }
            catch (ParseFailedException) {
                log.Warn("Failed parse options");
            }
            catch (ArgoException e) {
                log.Error(e);
                Environment.ExitCode = 1;
            }
            catch (Exception e) {
                log.Error($"Unexpected exception was thrown\n-->{e}");
                Environment.ExitCode = 1;
            }
        }
    }
}