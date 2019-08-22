using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Lupus.Repository;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;
using ShellProgressBar;
using Ptolemy.Map;

namespace Ptolemy.Lupus {
    [Verb("get", HelpText = "get data from database")]
    internal class Get : Verb.Verb {
        [Option('w', "sweepRange", Default = "1,5000", HelpText = "Sweepの範囲を[開始],[終了値]で指定できます")]
        public string SweepRange { get; set; }

        [Option('e', "seedRange", Default = "1,2000", HelpText = "Seedの範囲を[開始],[終了値]で指定できます")]
        public string SeedRange { get; set; }

        [Option('i', "sigmaRange", Default = "", HelpText = "シグマの範囲を[開始],[刻み幅],[終了値]で指定できます")]
        public string SigmaRange { get; set; }

        [Option("out", Default = "./out.csv", HelpText = "結果を出力するCSVファイルへのパスです")]
        public string OutFile { get; set; }

        private static IEnumerable<Tuple<string, long>> Push(Transistor vtn, Transistor vtp, Filter filter,
            long sweepStart, long sweepEnd,
            long seedStart, long seedEnd) {
            var dn = Transistor.ToTableName(vtn, vtp);

            var repo = new MssqlRepository();
            repo.Use(dn);
            return repo.Count(r => Within(sweepStart, sweepEnd, r.Sweep) && Within(seedStart, seedEnd, r.Seed),
                filter);
        }

        private static bool Within(long start, long end, long value) {
            return start <= value && value <= end;
        }

        protected override void Do(CancellationToken token) {
            // SiPrefixを考慮してオプションをパース
            var sigmaRange = SigmaRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (double) x.ParseDecimalWithSiPrefix()).ToArray();
            var sweepRange = SweepRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiPrefix()).ToArray();
            var seedRange = SeedRange.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (long) x.ParseDecimalWithSiPrefix()).ToArray();

            // 数え上げ用のFilterをBuild
            Logger.Info("Start build filter");
            var filter = new Filter(LupusConfig.Instance.Conditions, LupusConfig.Instance.Expressions);
            Logger.Info("Finished build filter");

            var result = new Map<string, Map<decimal, long>>();
            foreach (var s in filter.Delegates) {
                result[s.Name] = new Map<decimal, long>();
            }

            var sweepStart = sweepRange[0];
            var sweepEnd = sweepRange[1];
            var seedStart = seedRange[0];
            var seedEnd = seedRange[1];


            Logger.Info("Vtn:");
            Logger.Info($"\tVoltage: {VtnThreshold}");
            Logger.Info($"\tSigma: {VtnSigma}");
            Logger.Info($"\tDeviation: {VtnDeviation}");
            Logger.Info("Vtp:");
            Logger.Info($"\tVoltage: {VtpThreshold}");
            Logger.Info($"\tSigma: {VtpSigma}");
            Logger.Info($"\tDeviation: {VtpDeviation}");

            Logger.Info($"Sweeps: start: {sweepStart}, end: {sweepEnd}");
            Logger.Info($"Seed: start: {seedStart}, end: {seedEnd}");

            var sigmaList = new List<double>();

            if (sigmaRange.Length == 0) {
                Spinner.Start("Aggregating...", () => {

                    // Sigmaが固定
                    var vtn = new Transistor(VtnThreshold, VtnSigma, VtnDeviation);
                    var vtp = new Transistor(VtpThreshold, VtpSigma, VtpDeviation);

                    sigmaList.Add(VtnSigma);

                    var res = Push(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                    foreach (var (key, value) in res) {
                        result[key][vtn.Sigma] = value;
                    }
                });
            }
            else {
                // Sigmaを動かす
                var sigmaStart = sigmaRange[0];
                var sigmaStep = sigmaRange[1];
                var sigmaStop = sigmaRange[2];
                Logger.Info($"Range Sigma: start: {sigmaStart}, step: {sigmaStep}, stop: {sigmaStop}");

                for (var s = sigmaStart; s <= sigmaStop; s += sigmaStep) sigmaList.Add(s);

                using (var bar = new ProgressBar(sigmaList.Count, "Aggregating...", new ProgressBarOptions {
                    ForegroundColor = ConsoleColor.DarkBlue,
                    BackgroundCharacter = '-',
                    ForegroundColorDone = ConsoleColor.Green,
                    ProgressCharacter = '>',
                    BackgroundColor = ConsoleColor.DarkGray
                })) {
                    foreach (var sigma in sigmaList) {
                        token.ThrowIfCancellationRequested();

                        var vtn = new Transistor(VtnThreshold, sigma, VtnDeviation);
                        var vtp = new Transistor(VtpThreshold, sigma, VtpDeviation);

                        var res = Push(vtn, vtp, filter, sweepStart, sweepEnd, seedStart, seedEnd);
                        foreach (var (key, value) in res) {
                            result[key][vtn.Sigma] = value;
                        }

                        bar.Tick();
                    }
                }
            }

            // 出力
            Logger.Info("\n" + filter);
            var box = new List<IEnumerable<string>> {
                new[] {
                    "VtnThreshold",
                    "VtnSigma",
                    "VtnDeviation",
                    "VtpThreshold",
                    "VtpSigma",
                    "VtpDeviation",
                    "TotalSweeps"
                },
                new[] {
                    $"{VtnThreshold}",
                    $"{(sigmaRange.Length == 0 ? $"{VtnSigma}" : SigmaRange)}",
                    $"{VtnDeviation}",
                    $"{VtpThreshold}",
                    $"{(sigmaRange.Length == 0 ? $"{VtpSigma}" : SigmaRange)}",
                    $"{VtpDeviation}",
                    $"{(sweepEnd - sweepStart + 1) * (seedEnd - seedStart + 1)}"
                },
                new[] {
                    "Filter"
                }.Concat(sigmaList.Select(x => $"{x}"))
            };


            foreach (var (key, value) in result) {
                box.Add(new[] {key}.Concat(value.Select(x => $"{x.Value}")));
            }

            using (var sw = new StreamWriter(OutFile)) {
                foreach (var line in box) {
                    var item = string.Join(",", line);
                    sw.WriteLine(item);
                    Console.WriteLine(item);
                }
                sw.Flush();
            }

        }
    }
}
