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

        public IEnumerable<ResultEntity> Run(CancellationToken token, ArgoRequest request, IProgressBar bar = null) {

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

            var kind = HspiceOutKind.Else;
            var sweep = request.SweepStart;
            var records = (expect: sweep * request.Signals.Count * request.PlotTimeList.Count, actual: 0);
            var targetTimeList = request.PlotTimeList.Select(s => new ResultEntity {Time = s}).ToList();

            string line;
            while ((line = stdout.ReadLine()) != null || !p.HasExited) {
                token.ThrowIfCancellationRequested();


                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == 'x') {
                    kind = HspiceOutKind.Data;
                }
                else if (line[0] == 'y') {
                    sweep++;
                    kind = HspiceOutKind.Else;
                    bar?.Tick();
                }
                else if (line[0] == 't') continue;

                if (kind == HspiceOutKind.Else) continue;
                if (!line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                    .TryParseDecimalWithSiPrefix(out _)) continue;

                foreach (var resultEntity in
                    // パースして、時間がPlotTimeListに入っている奴だけ返す
                    ResultEntity.Parse(request.Seed, sweep, line, signals)
                        .Intersect(targetTimeList, new ResultEntityComparer())
                ) {
                    records.actual++;
                    yield return resultEntity;
                }
            }

            p.WaitForExit();
            if (p.ExitCode != 0) 
                throw new ArgoException($"Failed simulation: {p.StandardError.ReadToEnd()}");
            if (records.expect != records.actual)
                throw new ArgoException($"Record数が {records.expect} 個予期されていましたが、 {records.actual} 個しか出力されませんでした");
            File.Delete(spi);
        }

        private enum HspiceOutKind {
            Data,Else
        }
    }

    internal class ResultEntityComparer : IEqualityComparer<ResultEntity> {
        public bool Equals(ResultEntity x, ResultEntity y) {
            return x?.Time == y?.Time;
        }

        public int GetHashCode(ResultEntity obj) {
            return obj.GetHashCode();
        }
    }
}
