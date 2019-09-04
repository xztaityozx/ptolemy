using System;
using System.Collections.Generic;
using Ptolemy.Hydra;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.ToolTest{
    public class HydraRequestTest {
        [Fact]
        public void HydraDirectoryConstructTest() {
            var a = new HydraDirectories("sim","net", "res");
            Assert.Equal("sim", a.Simulation);
            Assert.Equal("net", a.NetList);
            Assert.Equal("res", a.Result);
        }

        [Fact]
        public void HydraRequestConstructTest() {
            var a = new HydraRequest("self", "result", new HydraDirectories("sim", "net", "res"), new HydraParameters {
                Seed = 1,
                Vtn = new Transistor(2M,3M,4M),
                Vtp = new Transistor(5M,6M,7M),
                SweepStart = 8,
                CelDirectory = "/path/to/cel",
                GndVoltage = 9,
                IcCommand = new List<string> { "icCommand" },
                ModelFile = "/path/to/modelfile",
                Sweeps = 10,
                VddVoltage = 11
            });
            Assert.NotNull(a);
            Assert.NotEqual(Guid.Empty, a.RequestId);
            Assert.Equal("self", a.SelfPath);
            Assert.Equal("result", a.ResultFile);
            Assert.Equal("sim", a.Directories.Simulation);
            Assert.Equal("net", a.Directories.NetList);
            Assert.Equal("res", a.Directories.Result);
            var param = a.Parameters;
            Assert.Equal(1, param.Seed);
            Assert.Equal(2M, param.Vtn.Threshold);
            Assert.Equal(3M, param.Vtn.Sigma);
            Assert.Equal(4M, param.Vtn.Deviation);
            Assert.Equal(5M, param.Vtp.Threshold);
            Assert.Equal(6M, param.Vtp.Sigma);
            Assert.Equal(7M, param.Vtp.Deviation);
            Assert.Equal(8, param.SweepStart);
            Assert.Equal("/path/to/cel", param.CelDirectory);
            Assert.Single(param.IcCommand);
            Assert.Equal("icCommand", param.IcCommand[0]);
            Assert.Equal("/path/to/modelfile", param.ModelFile);
            Assert.Equal(10, param.Sweeps);
            Assert.Equal(11, param.VddVoltage);
        }

    }
}