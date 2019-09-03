using System;
using Ptolemy.Hydra;
using Xunit;

namespace UnitTest.VerbTest {
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
            var a = new HydraRequest("self", "result", new HydraDirectories("sim", "net", "res"));
            Assert.NotNull(a);
            Assert.NotEqual(Guid.Empty, a.RequestId);
            Assert.Equal("self", a.SelfPath);
            Assert.Equal("result", a.ResultFile);
            Assert.Equal("sim", a.Directories.Simulation);
            Assert.Equal("net", a.Directories.NetList);
            Assert.Equal("res", a.Directories.Result);
        }
    }
}