using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ptolemy.Argo.Request;
using Ptolemy.Hydra.Processor;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.HydraTest {
    public class HydraJobTest {
        [Fact]
        public void FilePathTest() {
            var job = new HydraJob("path");
            Assert.Equal("path", job.ArgoRequestFilePath);
        }

        [Fact]
        public void IsSimulatableTest_True() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Hydra", "Test");
            Directory.CreateDirectory(tmp);
            var dummy = Path.Combine(tmp, "dummyFile");
            var taskFile = Path.Combine(tmp, "task.json");

            using (var sw = new StreamWriter(dummy)) sw.WriteLine("Dummy");
            using (var sw = new StreamWriter(taskFile))
                sw.WriteLine(new ArgoRequest {
                    HspicePath = dummy, Sweep = 1, Seed = 1, NetList = dummy, Gnd = 0,
                    Vdd = 1, HspiceOptions = new List<string>(), IcCommands = new List<string> {"V(a)=1", "V(b)=2"},
                    Includes = new List<string> {dummy}, PlotTimeList = new List<decimal> {1},
                    Signals = new List<string> {"a", "b"},
                    SweepStart = 1, Temperature = 25.0M, Transistors = new TransistorPair(1, 2, 3, 4, 5, 6.0),
                    Time = new RangeParameter(0, 1e-7M, 4E-12M)
                }.ToJson());

            try {
                var job = new HydraJob(taskFile);
                Assert.True(job.TryParseRequest(out _));
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void IsSimulatableTest_False() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Hydra", "Test");
            Directory.CreateDirectory(tmp);
            var dummy = Path.Combine(tmp, "dummyFile");
            var taskFile = Path.Combine(tmp, "task.json");

            using (var sw = new StreamWriter(dummy)) sw.WriteLine("Dummy");
            try {
                var job = new HydraJob(taskFile);
                Assert.False(job.TryParseRequest(out _));
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }
    }
}
