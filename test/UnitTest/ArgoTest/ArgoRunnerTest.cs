using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ptolemy.Argo;
using Ptolemy.Argo.Request;
using Ptolemy.FilePath;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.ArgoTest {
    public class ArgoRunnerTest {
        private static readonly string WorkingDir = Path.Combine(Path.GetTempPath(), "argoTest");
        private readonly ArgoRequest request = new ArgoRequest {
            Gnd = 0,
            Vdd = 0.8M,
            Seed = 1,
            Sweep = 100,
            Temperature = 25M,
            Time = new Range(0,100E-12M,20E-9M),
            Vtn = new Transistor(0.6,0.046,1.0),
            Vtp = new Transistor(-0.6,0.046,1.0),
            BaseDirectory = WorkingDir,
            GroupId = Guid.NewGuid(),
            HspiceOptions = new List<string>(),
            HspicePath = Environment.OSVersion.ToString().StartsWith("Unix") ?
                "seq 100 | xargs -I{} touch hspice.tr0@{} #" : "1..100|%{touch \"hspice.tr0@$_\"} #",
            IcCommands = new List<string>{"V(N1)=0.8V","V(N2)=0V"},
            SweepStart = 1,
            TargetCircuit = "path/to/circuit",
            ModelFilePath = Path.Combine(WorkingDir, "model")
        };
        [Fact]
        public void ConstructorTest() {
            var expect = new Runner(CancellationToken.None, request, "~/simulation");
            Assert.NotNull(expect);
        }

        [Fact]
        public void RunSuccessTest() {
            var runner = new Runner(CancellationToken.None, request, WorkingDir);

            void TryMkdir(string p) {
                p = FilePath.Expand(p);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            }


            TryMkdir(Path.Combine(WorkingDir, "path", "to", "circuit", "HSPICE", "nominal", "netlist", "cnl"));

            using (var sw = File.CreateText(request.ModelFilePath)) {
                sw.WriteLine("\nthis is model file for ArgoTest\n");
                sw.Flush();
            }
            using (var sw = File.CreateText(
                Path.Combine(WorkingDir, "path", "to", "circuit", "HSPICE", "nominal", "netlist", "netlist"))) {
                sw.WriteLine("\nthis is netlist file for ArgoTest\n");
                sw.Flush();
            }

            runner.Run();

            var wkr = Path.Combine(request.BaseDirectory, request.TargetCircuit.Replace("/", "_"),
                $"Vtn_{request.Vtn}", $"Vtp_{request.Vtp}");
            Assert.True(Directory.Exists(Path.Combine(wkr, "result")));
            Assert.True(Directory.Exists(Path.Combine(wkr, "netlist")));
            
            Directory.Delete(WorkingDir, true);
        }
    }
}