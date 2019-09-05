using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ptolemy.Hydra.Server;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.ToolTest {
    public class HydraRequestTest {
        [Fact]
        public void SerializeDeserializeTest() {
            var req = new HydraRequest
            {
                Vtn = new Transistor(1.0, 2.0, 3.0),
                Vtp = new Transistor(4.0, 5.0, 6.0),
                Seed = new Range<long>
                {
                    Start = 1,
                    Stop = 20,
                    Step = 1
                },
                SweepSplitOption = SweepSplitOption.SplitBySeed,
                SweepSplitSize = 5000,
                Sigma = new Range<decimal> {
                    Start = 0.046M,
                    Step = 0.004M,
                    Stop = 0.17M
                },
                Time = new Range<decimal> {
                    Start = 0,
                    Step = 100E-12M,
                    Stop = 20E-9M
                },
                PlotPoint = new Range<decimal> {
                    Start = 2.5E-9M,
                    Step = 7.5E-9M,
                    Stop = 17.5E-9M
                },
                Id = Guid.NewGuid(),
                KeepCsv = false,
                NotifyToSlackOnFinished = false,
                Signals = new List<string> { "A", "B", "C"},
                SlackUserName = "testUser",
                TargetCel = "user/cir/cel",
                TotalSweeps = (long)10E6,
                UseDatabase = false
            };

            var json = req.ToJson();
            var res = HydraRequest.FromJson(json);
            Assert.Equal($"{res}",$"{req}");

            using (var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(json)), Encoding.UTF8)) {
                Assert.Equal($"{res}", $"{HydraRequest.FromJson(sr)}");
            }
        }

        [Fact]
        public void SweepSplitOptionTest() {
            var data = new[] {
                new{json="{\"SweepSplitOption\":0}",exp=SweepSplitOption.NoSplit},
                new{json="{\"SweepSplitOption\":1}",exp=SweepSplitOption.SplitBySweep},
                new{json="{\"SweepSplitOption\":2}",exp=SweepSplitOption.SplitBySeed}
            };

            foreach (var d in data) {
                Assert.Equal(d.exp, HydraRequest.FromJson(d.json).SweepSplitOption);
            }
        }
    }
}
