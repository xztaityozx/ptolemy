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
    public class HspiceTest {
        [Fact]
        public void RunTest() {

            var hspice = new Hspice();
            var dir = Path.Combine(Path.GetTempPath(), "Ptolemy.Argo", "HspiceTest", "RunTest");
            FilePath.TryMakeDirectory(dir);
            var path = Path.Combine(dir, "out");
            var netlist = Path.Combine(dir, "netlist");
            using (var sw = new StreamWriter(netlist)) sw.WriteLine("x");

            try {
                var mock = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..",
                    "Script",
                    "hspice" + (Environment.OSVersion.Platform == PlatformID.Unix ? ".sh" : ".ps1"));
                Assert.True(File.Exists(mock));

                var res = hspice.Run(CancellationToken.None, new ArgoRequest {
                    Seed = 1, Sweep = 1, ResultFile = path, SweepStart = 1, Time = new RangeParameter(1, 2, 3),
                    NetList = netlist,
                    Signals = new List<string> {"x"}, GroupId = Guid.NewGuid(),
                    Transistors = new TransistorPair((1, 2, 3.0), (1, 2, 3.0)),
                    HspicePath = mock,
                    Gnd = 0, Vdd = 1, Includes = new List<string>(), HspiceOptions = new List<string>(),
                    IcCommands = new List<string>(), Temperature = 25
                });

            }
            finally {
                Directory.Delete(dir, true);
            }
        }
    }
}