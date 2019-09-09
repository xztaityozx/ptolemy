using System;
using System.Diagnostics;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    internal class Program {
        private static void Main(string[] args) {
            var p = new Process {
                StartInfo = new ProcessStartInfo {FileName = "bash", Arguments = "-c \"sleep 1000\""}
            };

            p.Start();
            p.Kill();
            Console.WriteLine(p.HasExited);
            Console.WriteLine(p.ExitCode);
        }
    }
}
