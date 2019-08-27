using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using Kurukuru;
using Microsoft.SqlServer.Server;
using Ptolemy.Lupus.Record;
using Ptolemy.Lupus.Repository;
using Ptolemy.Map;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;
using Ptolemy.Verb;
using ShellProgressBar;

namespace Ptolemy.Lupus {
    [Verb("get", HelpText = "DataBaseからデータを取り出し、コンフィグに従って数え上げを行います")]
    public class Get : Verb.Verb {

        [Option('i', "sigmaRange", Default = ",,", HelpText = "[開始値],[刻み幅],[終了値]でシグマを範囲指定します")]
        public string SigmaRange { get; set; }

        [Option('w', "sweepRange", Default = ",", HelpText = "[開始値],[終了値]でSweepの範囲を指定します")]
        public string SweepRange { get; set; }

        [Option('e', "seedRange",Default = ",", HelpText = "[開始値],[終了値]でSeedの範囲を指定します")]
        public string SeedRange { get; set; }

        [Option('o',"out", HelpText = "CSVを書き出すファイルへのパスです", Default = "./out.csv")]
        public string OutFile { get; set; }

        private (decimal start, decimal step, decimal stop) sigma;
        private (long start, long step, long stop) sweep, seed;

        private Exception BuildRangeValues() {
            try
            {
                var split = SigmaRange.Split(',').Zip(new[] { 0.046M, 0.004M, 0.2M },
                    (s, d) => string.IsNullOrEmpty(s) ? d : s.ParseDecimalWithSiPrefix()).ToArray();
                sigma = (split[0], split[1], split[2]);
            }
            catch (FormatException)
            {
                return new FormatException($"Format was invalid: {SigmaRange}");
            }
            catch (Exception e)
            {
                return e;
            }

            try
            {
                var split = SweepRange.Split(',').Zip(new[] { 1L, 5000L },
                    (s, d) => string.IsNullOrEmpty(s) ? d : s.ParseLongWithSiPrefix()).ToArray();
                sweep = (split[0], 1, split[1]);
            }
            catch (FormatException)
            {
                return new FormatException($"Format was invalid: {SweepRange}");
            }
            catch (Exception e)
            {
                return e;
            }

            try
            {
                var split = SeedRange.Split(',').Zip(new[] { 1L, 2000L },
                    (s, d) => string.IsNullOrEmpty(s) ? d : s.ParseLongWithSiPrefix()).ToArray();
                seed = (split[0], 1, split[1]);
            }
            catch (FormatException)
            {
                return new FormatException($"Format was invalid: {SeedRange}");
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        protected override Exception Do(CancellationToken token) {
            if(token.IsCancellationRequested) return new OperationCanceledException();
            {
                var res = BuildRangeValues();
                if (res != null) return res;
            }

            if (token.IsCancellationRequested) return new OperationCanceledException();
            var filter = new Filter(LupusConfig.Instance.Conditions, LupusConfig.Instance.Expressions);
            var request = SigmaRange.Split(',', StringSplitOptions.RemoveEmptyEntries).Length == 0
                ? new LupusGetRequest(Vtn, Vtp, filter, sweep, seed)
                : new LupusGetRequest(Vtn, Vtp, filter, sigma, sweep, seed);

            return token.IsCancellationRequested ? new OperationCanceledException() : GetFromDatabase(request, token);
        }

        private Exception GetRangeData(LupusGetRequest request,CancellationToken token) {
            try {
                var repo = new MssqlRepository();
                var list = request.GetTransistorEnumerable().ToArray();
                var result = new Map<string, long[]>();

                foreach (var d in request.Filter.Delegates) {
                    result[d.Name] = new long[list.Length];
                }

                using (var bar = new ProgressBar(list.Length, "Counting...", new ProgressBarOptions {
                    ProgressCharacter = '>',
                    BackgroundCharacter = '-',
                    CollapseWhenFinished = false,
                    ForegroundColor = ConsoleColor.DarkGreen
                })) {
                    foreach (var (vtn, vtp, idx) in list.Select((x, i) => Tuple.Create(x.Item1, x.Item2, i))) {
                        // cancel requested 
                        token.ThrowIfCancellationRequested();

                        // select database table
                        repo.Use(Transistor.ToTableName(vtn, vtp));

                        // Aggregate
                        foreach (var (signal, value) in repo.Count(r =>
                                request.SweepStart <= r.Sweep && r.Sweep <= request.SweepEnd &&
                                request.SeedStart <= r.Seed && r.Seed <= request.SeedEnd,
                            request.Filter)) {

                            // result[signal][indexOfSigma] = value
                            result[signal][idx] = value;
                        }

                        bar.Tick($"Aggregated: param={vtn},{vtp}");
                    }
                }

                var sb = new StringBuilder();
                // Parameter Header
                sb.AppendLine("Date");
                sb.AppendLine($"{DateTime.Now:yyyy MMMM dd}");
                sb.AppendLine(
                    $"{nameof(VtnThreshold)},{nameof(VtnDeviation)},{nameof(VtpThreshold)},{nameof(VtpDeviation)}");
                sb.AppendLine($"{VtnThreshold},{VtnDeviation},{VtpThreshold},{VtpDeviation}");
                sb.AppendLine("SweepStart,SweepEnd");
                sb.AppendLine($"{sweep.start},{sweep.stop}");
                sb.AppendLine("SeedStart,SeedEnd");
                sb.AppendLine($"{seed.start},{seed.stop}");

                // Table Header
                sb.AppendLine($"FilterName,{string.Join(",", request.GetSigmaEnumerable())}");
                foreach (var (key, values) in result) {
                    sb.AppendLine($"{key},{string.Join(",", values)}");
                }

                using (var sw = new StreamWriter(OutFile)) {
                    sw.WriteLine(sb.ToString());
                    sw.Flush();
                }
                Console.WriteLine(sb.ToString());
            }
            catch (OperationCanceledException e) {
                return new OperationCanceledException($"Canceled by user\n\t--> {e}");
            }
            catch (FileNotFoundException e) {
                return new FileNotFoundException($"Can not find {e.FileName}\n\t--> {e}");
            }
            catch (Exception e) {
                return e;
            }
            return null;
        }

        private Exception GetSingleData(LupusGetRequest request, CancellationToken token) {
            try {
                // throw
                token.ThrowIfCancellationRequested();

                var repo = new MssqlRepository();
                repo.Use(Transistor.ToTableName(request.Vtn, request.Vtp));
                
                // throw
                token.ThrowIfCancellationRequested();
                Tuple<string, long>[] result = null;
                Spinner.Start("Aggregating...", () => {
                    result = repo.Count(
                        r =>
                            request.SweepStart <= r.Sweep && r.Sweep <= request.SweepEnd &&
                            request.SeedStart <= r.Seed && r.Seed <= request.SeedEnd,
                        request.Filter);
                });

                // throw
                token.ThrowIfCancellationRequested();
                var sb = new StringBuilder();
                // Parameter Header
                sb.AppendLine("Date");
                sb.AppendLine($"{DateTime.Now:yyyy MMMM dd}");
                sb.AppendLine(
                    $"{nameof(VtnThreshold)},{nameof(VtnSigma)},{nameof(VtnDeviation)},{nameof(VtpThreshold)},{nameof(VtpSigma)},{nameof(VtpDeviation)}");
                sb.AppendLine(
                    $"{VtnThreshold},{VtnSigma},{VtnDeviation},{VtpThreshold},{VtpSigma},{VtpDeviation}");
                sb.AppendLine("FilterName,Value");
                foreach (var (key, value) in result) {
                    sb.AppendLine($"{key},{value}");
                }

                using (var sw = new StreamWriter(OutFile)) {
                    sw.WriteLine(sb.ToString());
                    sw.Flush();
                }

                Console.WriteLine(sb.ToString());
            }
            catch (OperationCanceledException e) {
                return new OperationCanceledException($"Canceled by User\n\t--> {e}");
            }
            catch (FileNotFoundException e) {
                return new FileNotFoundException($"Can not find {e.FileName}\n\t--> {e}");
            }
            catch (Exception e) {
                return e;
            }

            return null;
        }

        public Exception GetFromDatabase(LupusGetRequest request, CancellationToken token) {
            switch (request.Mode) {
                case LupusGetRequest.RequestMode.Range:
                    return GetRangeData(request, token);
                case LupusGetRequest.RequestMode.Single:
                    return GetSingleData(request, token);
                default:
                    return new Exception($"Failed Parse Request: {request.ToJson()}");
            }
        }
    }
}
