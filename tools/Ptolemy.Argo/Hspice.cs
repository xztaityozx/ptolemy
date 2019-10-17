using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Argo {
    public class Hspice {
        private readonly string workingRoot;

        public Hspice() {
            workingRoot = Path.Combine(Path.GetTempPath(), "Ptolemy.Argo");
        }

        public List<ResultEntity> Run(CancellationToken token, ArgoRequest request) {
            var rt = new List<ResultEntity>();

            var guid = Guid.NewGuid();
            var dir = Path.Combine(workingRoot, $"{request.GroupId}");
            FilePath.FilePath.TryMakeDirectory(dir);
            Directory.SetCurrentDirectory(dir);
            var spi = Path.Combine(dir, guid + ".spi");

            try {
                request.WriteSpiScript(spi);

                using var p = new Process {
                    StartInfo = new ProcessStartInfo {
                        UseShellExecute = false,
                        FileName = "bash",
                        ArgumentList =
                            {"-c", $"{request.HspicePath} {string.Join(" ", request.HspiceOptions)} -i {guid}.spi"},
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                p.Start();
                var stdout = p.StandardOutput;

                var signals = request.Signals;

                var kind = HspiceOutKind.Else;
                var sweep = request.SweepStart;
                string line;
                while ((line = stdout.ReadLine()) != null || !token.IsCancellationRequested && !p.HasExited) {

                    if (string.IsNullOrEmpty(line)) continue;
                    if (line[0] == 'x') {
                        kind = HspiceOutKind.Data;
                    }
                    else if (line[0] == 'y') {
                        sweep++;
                        kind = HspiceOutKind.Else;
                    }

                    if (kind == HspiceOutKind.Else) continue;
                    if (!line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                        .TryParseDecimalWithSiPrefix(out _)) continue;

                    //Console.WriteLine(line);

                    rt.AddRange(ResultEntity.Parse(request.Seed, sweep, line, signals));
                }

                p.WaitForExit();
            }
            finally {
                File.Delete(spi);
            }
            

            return rt;
        }

        private enum HspiceOutKind {
            Data,Else
        }
    }
}
