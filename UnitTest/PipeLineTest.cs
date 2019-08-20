using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    public class PipeLineTest {
        [Fact]
        public void ConstructTest() {
            using (var pipeline = new PipeLine.PipeLine(CancellationToken.None)) {
                Xunit.Assert.NotNull(pipeline);
            }
        }

        [Fact]
        public void InitTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var first = p.Init(Enumerable.Range(0, 100), 10, 20, i => i + 1);
                Xunit.Assert.Null(first.Next);
                var status = p.Start(() => {
                    CollectionAssert
                        .AreEquivalent(Enumerable.Range(0, 100).Select(i => i + 1).ToList(), first.Out.ToList());
                });
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);

            }
        }

        [Fact]
        public void InitSelectManyTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var first = p.InitSelectMany(Enumerable.Range(0, 100), 10, 20, i => Enumerable.Repeat(i, 3));
                Xunit.Assert.Null(first.Next);
                var status = p.Start(() => {
                    var expect = Enumerable.Range(0, 100).SelectMany(i => Enumerable.Repeat(i, 3)).ToList();
                    var actual = first.Out.ToList();
                    CollectionAssert.AreEquivalent(expect, actual);
                });
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);

            }
        }

        [Fact]
        public void ThenTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var first = p.Init(Enumerable.Range(0, 100), 10, 20, i => i + 1);
                var second = first.Then(10, 20, i => $"{i}");
                var third = second.ThenSelectMany(10, 20, i => Enumerable.Repeat(i, 3));
                Xunit.Assert.Same(first.Next, second);
                Xunit.Assert.Same(second.Next, third);
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i + 1).Select(x => $"{x}")
                            .SelectMany(x => Enumerable.Repeat(x, 3)).ToList(),
                        third.Out.ToList()
                    );
                });
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnBeginTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var onBegin = false;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, () => onBegin = true).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.ToList()
                    );
                });
                Xunit.Assert.True(onBegin);
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnIntervalTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var onInterval = 0;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, null, null, _ => onInterval++).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.ToList()
                    );
                });
                Xunit.Assert.Equal(100, onInterval);
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnInnerIntervalTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var onInnerInterval = 0;
                var res = p.InitSelectMany(Enumerable.Range(0, 100), 10, 10, i => Enumerable.Range(i, 3),
                    null, null, null,
                    _ => onInnerInterval++).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).SelectMany(i => Enumerable.Range(i, 3)).ToList(),
                        res.ToList()
                    );
                });
                Xunit.Assert.Equal(300, onInnerInterval);
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnFinishTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                var onFinish = false;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, null, () => onFinish = true);
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.Out.ToList());
                });
                Xunit.Assert.Equal(PipeLine.PipeLine.PipeLineStatus.Completed, status);
                Xunit.Assert.True(onFinish);
            }
        }

        [Fact]
        public void ExceptionTest() {
            using (var p = new PipeLine.PipeLine(CancellationToken.None)) {
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => p.Start(() => { }));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => p.Init(new[] { 1 }, 0, 1, i => i));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => p.Init(new[] { 1 }, 1, 0, i => i));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => p.Init(new[] { 1 }, 0, 0, i => i));
                var first = p.Init(new[] {1}, 1, 1, i => i);
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.Then(0, 1, i => i));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.Then(0, 0, i => i));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.Then(1, 0, i => i));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.ThenSelectMany(0, 1, i => Enumerable.Repeat(i,3)));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.ThenSelectMany(0, 0, i => Enumerable.Repeat(i,3)));
                Xunit.Assert.Throws<PipeLine.PipeLineException>(() => first.ThenSelectMany(1, 0, i => Enumerable.Repeat(i,3)));
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource(0)) {
                using (var p = new PipeLine.PipeLine(cts.Token)) {
                    p.Init(Enumerable.Range(0,10000), 1, 1, i => i);
                    Xunit.Assert.Throws<TaskCanceledException>(() => p.Start(null));
                }
            }
        }

        [Fact]
        public void DisposeTest() {
            var p=new PipeLine.PipeLine(CancellationToken.None);
            var res = p.Init(new[] {1}, 1, 1, i => i * i).Out;
            p.Start(null);
            p.Dispose();
            Xunit.Assert.Throws<ObjectDisposedException>(() => res.ElementAt(0));
        }
    }
}
