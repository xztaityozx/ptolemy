using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using CommandLine;
using Kurukuru;
using ShellProgressBar;

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
                    }, e => throw new ArgoParseFailedException());
                var results = new List<StringBuilder>();

                Console.Clear();
                log.Info($"Target Netlist: {req.NetList}");
                log.Info("Parameters");
                log.Info($"\tVtn:");
                log.Info($"\t\tThreshold: {req.Transistors.Vtn.Threshold}");
                log.Info($"\t\tSigma: {req.Transistors.Vtn.Sigma}");
                log.Info($"\t\tDeviation: {req.Transistors.Vtn.Deviation}");
                log.Info($"\tVtp:");
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

                using (var cts = new CancellationTokenSource()) {
                    Console.CancelKeyPress += (sender, eventArgs) => {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };
                    bool status;
                    var watch = new Stopwatch();
                    var argo = new Argo(req, cts.Token);
                    using (var pb = new ProgressBar((int) req.Sweep, "Ptolemy.Argo", new ProgressBarOptions {
                        BackgroundCharacter = '-',
                        BackgroundColor = ConsoleColor.DarkGray,
                        ForegroundColor = ConsoleColor.DarkBlue,
                        ProgressCharacter = '>',
                        ForegroundColorDone = ConsoleColor.Green,
                        CollapseWhenFinished = false
                    })) {
                        var ob = argo.Receiver.Subscribe(s => {
                            if (s[0] == 'x') {
                                pb.Tick();
                                results.Add(new StringBuilder());
                            }
                            else {
                                results.Last().AppendLine(s);
                            }
                        });
                        cts.Token.Register(ob.Dispose);
                        watch.Start();
                        (status, _) = argo.Run();
                        watch.Start();
                    }

                    if (!status) {
                        log.Error("Failed simulation");
                    }
                    else {
                        log.Info("Finished simulation");
                        log.Info($"Elapsed time: {watch.Elapsed}");
                        log.Info("Result file: " + req.ResultFile +
                                 $"[{req.SweepStart}..{req.Sweep + req.SweepStart - 1}]");
                        foreach (var item in results.Select((sb, i) => new {sb, i = i + req.SweepStart})) {
                            var path = req.ResultFile + $"{item.i}";
                            using (var sw = new StreamWriter(path, false, new UTF8Encoding(false))) {
                                sw.WriteLine(item.sb.ToString().TrimEnd());
                                sw.Flush();
                            }
                        }
                    }
                }
            }
            catch (ArgoClean) {
                log.Info("Ptolemy.Argo clean temp directory /tmp/Ptolemy.Argo");
            }
            catch (ArgoParseFailedException) {
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