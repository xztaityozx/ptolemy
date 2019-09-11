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
                e.Run("echo abc");
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public void RunWithStdOutTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Run("echo abc", s => Assert.Equal("abc", s.TrimEnd()));
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public void RunWithStdErrTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Run("EXEC_TEST_NOT_FOUND", s => { }, Assert.NotNull, false);
            }
        }

        [Fact]
        public void RunCombineOutput() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Run("echo abc; EXEC_TEST_NOT_FOUND", Assert.NotNull, true);
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource(100)) {
                using (var e = new Exec(cts.Token)) {
                    e.Run("sleep 10000");
                    Assert.NotEqual(0, e.ExitCode);
                }
            }
        }
    }
}
