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
        public void StartTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Start("echo abc");
                e.Wait();
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public async Task RunAsyncTest() {
            using (var e = new Exec(CancellationToken.None)) {
                await e.RunAsync("echo abc");
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public void RunTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Run("echo abc");
                Assert.Equal(0, e.ExitCode);
            }
        }

        [Fact]
        public void StdoutPipeTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Start("echo abc");
                var box = e.StdOutPipe.Select(x => x.TrimEnd()).ToList();

                Assert.Single(box);
                Assert.Equal("abc", box[0]);
            }
        }

        [Fact]
        public void StderrPipeTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Start("echo abc");
                Assert.Empty(e.StdErrPipe);
            }
        }

        [Fact]
        public void TimeoutTest() {
            using (var e = new Exec(CancellationToken.None)) {
                e.Start("sleep 1000");
                Assert.False(e.Wait(100));
            }
        }

        [Fact]
        public void CombineOutputTest() {
            using (var e = new Exec(CancellationToken.None, true)) {
                e.Start("echo abc;HYDRA_TEST_NOT_FOUND");
                Assert.NotEmpty(e.StdOutPipe);
                Assert.Empty(e.StdErrPipe);
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource()) {
                cts.Cancel();
                using (var e = new Exec(cts.Token)) {
                    e.Run("sleep 1000");
                    Assert.NotEqual(0, e.ExitCode);
                }
            }
        }
    }
}
