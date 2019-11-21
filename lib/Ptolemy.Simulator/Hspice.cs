using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;

namespace Ptolemy.Simulator {
    /// <summary>
    /// Hspiceを直接起動するクラス
    /// </summary>
    public class Hspice : ISimulator {
        private readonly string workingRoot;

        public Hspice() {
            workingRoot = Path.Combine(Path.GetTempPath(), "Ptolemy", "Hspice");
        }

        public IReadOnlyList<ResultEntity> Run(CancellationToken token, ArgoRequest request, Action intervalAction) {
            var guid = Guid.NewGuid();
            var dir = Path.Combine(workingRoot, $"{request.GroupId}");
            FilePath.FilePath.TryMakeDirectory(dir);
            Directory.SetCurrentDirectory(dir);
            var spi = Path.Combine(dir, guid + ".spi");

            request.WriteSpiScript(spi);

            using var p = new Process {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "bash" : "powershell.exe",
                    ArgumentList =
                        {"-c", $"{request.HspicePath} {string.Join(" ", request.HspiceOptions)} -i {guid}.spi"},
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            if (!p.Start()) throw new SimulatorException("Failed start hspice");
            var stdout = p.StandardOutput;

            var signals = request.Signals;

            var sweep = request.SweepStart;
            var records = (expect: request.ExpectedRecords, actual: 0);

            var rt = new List<ResultEntity>();
            string line;
            while ((line = stdout.ReadLine()) != null || !p.HasExited) {
                token.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == 'y') {
                    sweep++;
                    intervalAction?.Invoke();
                }

                if (!TryParseOutput(request.Seed, sweep, line, signals, out var o)) continue;
                foreach (var resultEntity in o.Intersect(request.PlotTimeList, re => re.Time)) {
                    records.actual++;
                    rt.Add(resultEntity);
                }
            }

            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new SimulatorException($"Failed simulation: {p.StandardError.ReadToEnd()}");
            if (records.expect != records.actual)
                throw new SimulatorException($"Record数が {records.expect} 個予期されていましたが、 {records.actual} 個しか出力されませんでした");
            File.Delete(spi);

            return rt;

        }

        private static bool TryParseOutput(long seed, long sweep, string line, IEnumerable<string> signals,
            out IEnumerable<ResultEntity> o) {
            try {
                o = ResultEntity.Parse(seed, sweep, line, signals);
                return true;
            }
            catch (Exception) {
                o = null;
                return false;
            }
        }

    }

    internal static class IntersectExt {
        public static IEnumerable<TItem> Intersect<TItem, TKey>(this IEnumerable<TItem> @this,
            IEnumerable<TKey> second, Func<TItem, TKey> selector) {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var map = new Map.Map<TKey, bool>(false);
            foreach (var key in second) {
                map[key] = true;
            }

            foreach (var item in @this) {
                if (map[selector(item)]) yield return item;
            }
        }
    }
}
