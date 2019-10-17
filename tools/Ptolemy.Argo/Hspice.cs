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
        private readonly string hspice, workingRoot;

        public Hspice(string hspicePath, string workingRoot) {
            hspice = hspicePath;
            this.workingRoot = workingRoot;
        }

        public List<ResultEntity> Run(CancellationToken token, ArgoRequest request) {
            var rt = new List<ResultEntity>();

            Directory.CreateDirectory(workingRoot);
            Directory.SetCurrentDirectory(workingRoot);

            using (var p = new Process {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = "bash",
                    ArgumentList = {"-c", "~/TestDir/hspice.sh"},
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
            }) {
                p.Start();
                var stdout = p.StandardOutput;

                var stderr = p.StandardError;

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
                    var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (!split[0].TryParseDecimalWithSiPrefix(out _)) continue;

                    Console.WriteLine(line);

                    //rt.AddRange(ResultEntity.Parse(request.Seed, sweep, line, signals));
                }

                p.WaitForExit();
            }

            return rt;
        }

        private enum HspiceOutKind {
            Data,Else
        }
    }
}
