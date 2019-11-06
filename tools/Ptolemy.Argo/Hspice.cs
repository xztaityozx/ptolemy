using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using Ptolemy.SiMetricPrefix;
using ShellProgressBar;

namespace Ptolemy.Argo {
    public class Hspice {
        private readonly string workingRoot;

        public Hspice() {
            workingRoot = Path.Combine(Path.GetTempPath(), "Ptolemy.Argo");
        }

        public IReadOnlyList<ResultEntity> Run(CancellationToken token, ArgoRequest request, IProgressBar bar = null) {

            var guid = Guid.NewGuid();
            var dir = Path.Combine(workingRoot, $"{request.GroupId}");
            FilePath.FilePath.TryMakeDirectory(dir);
            Directory.SetCurrentDirectory(dir);
            var spi = Path.Combine(dir, guid + ".spi");

            request.WriteSpiScript(spi);

            using var p = new Process {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "bash":"powershell.exe",
                    ArgumentList =
                        {"-c", $"{request.HspicePath} {string.Join(" ", request.HspiceOptions)} -i {guid}.spi"},
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            if(!p.Start()) throw new ArgoException("Failed start hspice");
            var stdout = p.StandardOutput;

            var signals = request.Signals;

            var sweep = request.SweepStart;
            var records = (expect: request.ExpectedRecords, actual: 0);
            var targetTimeList = request.PlotTimeList.Select(s => new ResultEntity { Time = s }).ToList();

            var rt = new List<ResultEntity>();
            string line;
            while ((line = stdout.ReadLine()) != null || !p.HasExited) {
                token.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == 'y') {
                    sweep++;
                    bar?.Tick();
                }

                if (!TryParseOutput(request.Seed, sweep, line, signals, out var o)) continue;
                foreach (var resultEntity in o.Intersect(targetTimeList, new ResultEntityComparer())) {
                    records.actual++;
                    rt.Add(resultEntity);
                }
            }

            p.WaitForExit();
            if (p.ExitCode != 0) 
                throw new ArgoException($"Failed simulation: {p.StandardError.ReadToEnd()}");
            if (records.expect != records.actual)
                throw new ArgoException($"Record数が {records.expect} 個予期されていましたが、 {records.actual} 個しか出力されませんでした");
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

    internal class ResultEntityComparer : IEqualityComparer<ResultEntity> {
        public bool Equals(ResultEntity x, ResultEntity y) {
            return x != null && y != null && x.Time.Equals(y.Time);
        }

        public int GetHashCode(ResultEntity obj) {
            return obj.Time.GetHashCode();
        }
    }
}
