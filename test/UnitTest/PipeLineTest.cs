using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.PipeLine;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest
{
    public class PipeLineTest {
        [Fact]
        public void ConstructTest() {
            using (var pipeline = new PipeLine(CancellationToken.None)) {
                Assert.NotNull(pipeline);
            }
        }

        [Fact]
        public void InitTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.Init(Enumerable.Range(0, 100), 10, 20, i => i + 1);
                Assert.Null(first.Next);
                var status = p.Start(() => {
                    CollectionAssert
                        .AreEquivalent(Enumerable.Range(0, 100).Select(i => i + 1).ToList(), first.Out.ToList());
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);

            }
        }

        [Fact]
        public void InitSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.InitSelectMany(Enumerable.Range(0, 100), 10, 20, i => Enumerable.Repeat(i, 3));
                Assert.Null(first.Next);
                var status = p.Start(() => {
                    var expect = Enumerable.Range(0, 100).SelectMany(i => Enumerable.Repeat(i, 3)).ToList();
                    var actual = first.Out.ToList();
                    CollectionAssert.AreEquivalent(expect, actual);
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);

            }
        }

        [Fact]
        public void ThenTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.Init(Enumerable.Range(0, 100), 10, 20, i => i + 1);
                var second = first.Then(10, 20, i => $"{i}");
                var third = second.ThenSelectMany(10, 20, i => Enumerable.Repeat(i, 3));
                Assert.Same(first.Next, second);
                Assert.Same(second.Next, third);
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i + 1).Select(x => $"{x}")
                            .SelectMany(x => Enumerable.Repeat(x, 3)).ToList(),
                        third.Out.ToList()
                    );
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnBeginTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var onBegin = false;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, () => onBegin = true).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.ToList()
                    );
                });
                Assert.True(onBegin);
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnIntervalTest() {
            using(var box=new BlockingCollection<object>())
            using (var p = new PipeLine(CancellationToken.None)) {
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, null, null, _ => box.Add(_)).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.ToList()
                    );
                });
                box.CompleteAdding();
                Assert.Equal(100, box.GetConsumingEnumerable().Count());
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnInnerIntervalTest() {
            using(var box=new BlockingCollection<int>())
            using (var p = new PipeLine(CancellationToken.None)) {
                var res = p.InitSelectMany(Enumerable.Range(0, 100), 10, 10, i => Enumerable.Range(i, 3),
                    null, null, null,
                    _ => box.Add((int)_)).Out;
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).SelectMany(i => Enumerable.Range(i, 3)).ToList(),
                        res.ToList()
                    );
                });
                box.CompleteAdding();
                Assert.Equal(300, box.GetConsumingEnumerable().Count());
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);
            }
        }

        [Fact]
        public void OnFinishTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var onFinish = false;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i, null, () => onFinish = true);
                var status = p.Start(() => {
                    CollectionAssert.AreEquivalent(
                        Enumerable.Range(0, 100).Select(i => i * i).ToList(),
                        res.Out.ToList());
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, status);
                Assert.True(onFinish);
            }
        }

        [Fact]
        public void ExceptionTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                Assert.Throws<PipeLineException>(() => p.Start(() => { }));
                Assert.Throws<PipeLineException>(() => p.Init(new[] { 1 }, 0, 1, i => i));
                Assert.Throws<PipeLineException>(() => p.Init(new[] { 1 }, 1, 0, i => i));
                Assert.Throws<PipeLineException>(() => p.Init(new[] { 1 }, 0, 0, i => i));
                var first = p.Init(new[] {1}, 1, 1, i => i);
                Assert.Throws<PipeLineException>(() => first.Then(0, 1, i => i));
                Assert.Throws<PipeLineException>(() => first.Then(0, 0, i => i));
                Assert.Throws<PipeLineException>(() => first.Then(1, 0, i => i));
                Assert.Throws<PipeLineException>(() => first.ThenSelectMany(0, 1, i => Enumerable.Repeat(i,3)));
                Assert.Throws<PipeLineException>(() => first.ThenSelectMany(0, 0, i => Enumerable.Repeat(i,3)));
                Assert.Throws<PipeLineException>(() => first.ThenSelectMany(1, 0, i => Enumerable.Repeat(i,3)));
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource(0)) {
                using (var p = new PipeLine(cts.Token)) {
                    p.Init(Enumerable.Range(0,10000), 1, 1, i => i);
                    Assert.Throws<TaskCanceledException>(() => p.Start(null));
                }
            }
        }

        [Fact]
        public void DisposeTest() {
            var p=new PipeLine(CancellationToken.None);
            var res = p.Init(new[] {1}, 1, 1, i => i * i).Out;
            p.Start(null);
            p.Dispose();
            Assert.Throws<ObjectDisposedException>(() => res.ElementAt(0));
        }
    }
}
