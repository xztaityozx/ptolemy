using System.Linq;
using Ptolemy.Hydra.Simulation;
using Xunit;

namespace UnitTest.ToolTest {
    public class SimulationTest {
        [Fact]
        public void HspiceGetCommandTest() {
            var data = new[] {
                new {
                    expect = "/path/to/hspice option -i spi -o ./hspice &> ./hspice.log", path = "/path/to/hspice",
                    opt = new[] {"option"}, spi = "spi"
                },
                new {
                    expect = "h a b c -i s -o ./hspice &> ./hspice.log", path = "h",
                    opt = new[]{"a","b","c"}, spi = "s"
                }
            };
            foreach (var d in data) {
                Assert.Equal(d.expect, new Hspice{Path = d.path, Options = d.opt.ToList()}.GetCommand(d.spi));
            }
        }

        [Fact]
        public void WaveViewGetCommandTest() {
            var data = new[] {
                new {expect = "/path/to/wv -k -ace_no_gui ace &> ./wv.log", path = "/path/to/wv", ace = "ace"}
            };

            foreach (var d in data) {
                Assert.Equal(d.expect, new WaveView{Path = d.path}.GetCommand(d.ace));
            }
        }

        [Fact]
        public void SimulationRequestTest() {
            var actual = new SimulationRequest(new Hspice{Path = "hspice"}, new WaveView{Path = "wv"}, "sd", "rd", "spi", "ace", false, true);
            Assert.Equal("hspice", actual.Hspice.Path);
            Assert.Equal("wv", actual.WaveView.Path);
            Assert.Equal("rd", actual.ResultDir);
            Assert.Equal("sd", actual.SimulationDir);
            Assert.Equal("spi", actual.SpiScript);
            Assert.Equal("ace", actual.AceScript);
            Assert.False(actual.KeepCsv);
            Assert.True(actual.AutoRemove);
        }
    }
}