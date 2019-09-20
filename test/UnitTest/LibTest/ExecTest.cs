using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Exec;
using Xunit;

namespace UnitTest.LibTest {
    public class ExecTest {
        [Fact]
        public void ConstructorTest() {
            Assert.NotNull(new Exec(CancellationToken.None));
        }

        [Fact]
        public void RunTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Run("echo",new []{"abc"});
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public void RunWithStdOutTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.StdOut.Subscribe(s => Assert.Equal("abc", s.TrimEnd()));
                e.Run("echo", new[]{"abc"});
            }
        }

        [Fact]
        public void ThrowTest() {
            using (var e = new Exec(CancellationToken.None)) {
                Assert.Throws<InvalidOperationException>(() => e.Run("EXEC_TEST_NOT_FOUND", new[]{""}));
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource(100)) {
                using (var e = new Exec(cts.Token)) {
                    e.Run("sleep", new[]{"10000"});
                    Assert.NotEqual(0, e.ExitCode);
                }
            }
        }
    }
}
