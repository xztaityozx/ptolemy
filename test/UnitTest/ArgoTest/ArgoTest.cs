using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CommandLine;
using Newtonsoft.Json;
using Ptolemy.Argo;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.ArgoTest {
    public class ArgoTest {
        [Fact]
        public void RunTest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Argo");
            Directory.CreateDirectory(tmp);
            var hspice = Path.Combine(Directory.GetCurrentDirectory(),"..","..","..", "Script",
                Environment.OSVersion.ToString().StartsWith("Unix") ? "hspice.sh" : "hspice.ps1");
            var req = new ArgoRequest {
                HspicePath = hspice,
                NetList = hspice,
                Transistors = new TransistorPair(1.0,2,3,4,5,6),
                Gnd = 0,
                Includes = new List<string>(),
                Seed = 1,
                Signals = new List<string>(),
                Sweep = 1,
                Temperature = 1,
                Time = new Range(1,2,3),
                Vdd = 1,
                HspiceOptions = new List<string>(),
                ResultFile = "",
                SweepStart = 1,
                IcCommands = new List<string>()
            };
            
            var argo = new Argo(req, CancellationToken.None);
            var sb = new StringBuilder();
            argo.Receiver.Subscribe(s => sb.AppendLine(s));

            var (status, res) = argo.Run();
            
            Assert.True(status);
            Assert.Equal(JsonConvert.SerializeObject(req), JsonConvert.SerializeObject(res));

            Assert.Equal(@"x
1
2
3
4
5
6
7
8
9
10
", sb.ToString());
            
            Directory.Delete(tmp, true);
        }

        [Fact]
        public void CleanTest() {
        }
    }
}