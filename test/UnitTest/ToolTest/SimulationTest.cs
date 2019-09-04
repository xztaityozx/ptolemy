using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ptolemy.Hydra;
using Ptolemy.Hydra.Simulation;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.ToolTest {
    public class SimulationTest {
        [Fact]
        public void SimulationToolTest1()
        {
            using (var st = new SimulationTool(CancellationToken.None, "echo"))
            {
                Assert.NotNull(st);
                Assert.Equal("echo", st.path);
                Assert.Equal(0, st.Run("abc"));
                Assert.Equal("abc", st.StdOut.ReadToEnd().TrimEnd());
                Assert.Equal("", st.StdError.ReadToEnd().TrimEnd());
            }
        }

        [Fact]
        public void SimulationToolCancelTest() {
            using (var cts = new CancellationTokenSource(5))
            using (var st = new SimulationTool(cts.Token, "sleep")) {
                Assert.NotNull(st);
                Assert.Equal("sleep", st.path);
                Assert.Equal(-1, st.Run("500"));
            }
        }

        [Fact]
        public void SimulationToolCommandNotFoundTest() {
            using (var st = new SimulationTool(CancellationToken.None, "HydraTest_CommandNotFound")) {
                Assert.NotNull(st);
                Assert.Equal(1, st.Run(""));
            }
        }
    }
}