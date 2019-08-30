using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.PipeLine;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest.LibTest
{
    public class PipeLineTest {
        [Fact]
        public void ConstructorTest() {
            Assert.NotNull(new PipeLine(CancellationToken.None));
        }

        [Fact]
        public void InitTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i);
                Assert.NotNull(x);
                Assert.Null(x.Next);
                var s = p.Start(() => { Assert.Equal(4950, x.Out.Sum()); });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void InitSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.InitSelectMany(Enumerable.Range(0, 100), 10, 10, i => Enumerable.Repeat(i, 3));
                Assert.NotNull(x);
                Assert.Null(x.Next);
                var s = p.Start(() => Assert.Equal(4950 * 3, x.Out.Sum()));
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void ThenTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i);
                var y = x.Then(10, 10, i => i * i);

                Assert.NotSame(x, y);
                Assert.Same(y, x.Next);

                var s = p.Start(() => Assert.Equal(Enumerable.Range(0, 100).Select(i => i * i).Sum(), y.Out.Sum()));
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void ThenSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None))
            {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i*i);
                var y = x.ThenSelectMany(10, 10, i => Enumerable.Repeat(i, 10));

                Assert.NotSame(x, y);
                Assert.Same(y, x.Next);

                var s = p.Start(() => Assert.Equal(Enumerable.Range(0, 100).Select(i => i * i * 10).Sum(), y.Out.Sum()));
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void ThenBufferTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i);
                var y = x.Then(10, 10, i => i + 1);
                var z = y.Buffer(10, 100);

                Assert.NotSame(x, y);
                Assert.Same(y, x.Next);
                Assert.Same(z, y.Next);

                var s = p.Start(() => {
                    var idx = 0;
                    var sum = 0L;
                    foreach (var item in z.Out) {
                        sum += item.Sum();
                        idx++;
                    }
                    Assert.Equal(10, idx);
                    Assert.Equal(Enumerable.Range(0, 100).Select(i => i * i + 1).Sum(), sum);
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void ThenThenTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i);
                var y = x.Then(10, 10, i => i + 1);
                var z = y.Then(10,10,i=>i*2);

                Assert.NotSame(x, y);
                Assert.Same(y, x.Next);
                Assert.Same(z, y.Next);

                var s = p.Start(() => {
                    Assert.Equal(
                        Enumerable.Range(0, 100).Select(i => (i * i + 1)*2).Sum(),
                        z.Out.Sum()
                    );
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void ThenThenSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None))
            {
                var x = p.Init(Enumerable.Range(0, 100), 10, 10, i => i * i);
                var y = x.Then(10, 10, i => i + 1);
                var z = y.ThenSelectMany(10, 10, i => Enumerable.Repeat(i, 3));

                Assert.NotSame(x, y);
                Assert.Same(y, x.Next);
                Assert.Same(z, y.Next);

                var s = p.Start(() => {
                    Assert.Equal(
                        Enumerable.Range(0,100).Select(i=> (i * i + 1)*3).Sum(),
                        z.Out.Sum()
                    );
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }

        }

        [Fact]
        public void ThenSelectManyBufferTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.InitSelectMany(Enumerable.Range(0, 100), 10, 10, i => Enumerable.Repeat(i, 3));
                var y = x.Buffer(10, 10);
                Assert.NotSame(x,y);
                Assert.Same(y, x.Next);
                Assert.Null(y.Next);

                var s = p.Start(() => {
                    Assert.Equal(
                        Enumerable.Range(0,100).SelectMany(i=>Enumerable.Repeat(i,3)).Sum(),
                        y.Out.SelectMany(i=>i).Sum()
                    );
                });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void DisposeTest() {
            var p = new PipeLine(CancellationToken.None);
            var x = p.Init(Enumerable.Range(0, 10), 10, 10, i => i);
            p.Dispose();
            Assert.Throws<ObjectDisposedException>(() => x.Out.ToArray());
        }

        [Fact]
        public void Exceptions() {
            using (var p = new PipeLine(CancellationToken.None)) {
                Assert.Throws<PipeLineException>(() => p.Init(Enumerable.Range(0, 10), 0, 0, i => i));
                Assert.Throws<PipeLineException>(() => p.Init(Enumerable.Range(0, 10), 1, 0, i => i));
                Assert.Throws<PipeLineException>(() => p.Init(Enumerable.Range(0, 10), 0, 1, i => i));
                var init = p.Init(Enumerable.Range(0, 10), 10, 10, i => i);
                Assert.Throws<PipeLineException>(() => init.Then(0, 0, i => i * i));
                Assert.Throws<PipeLineException>(() => init.Then(0, 1, i => i * i));
                Assert.Throws<PipeLineException>(() => init.Then(1, 0, i => i * i));
                Assert.Throws<PipeLineException>(() => init.ThenSelectMany(0, 0, i => $"{i}"));
                Assert.Throws<PipeLineException>(() => init.ThenSelectMany(0, 1, i => $"{i}"));
                Assert.Throws<PipeLineException>(() => init.ThenSelectMany(1, 0, i => $"{i}"));
                Assert.Throws<PipeLineException>(() => init.Buffer(0, 0));
                Assert.Throws<PipeLineException>(() => init.Buffer(0, 1));
                Assert.Throws<PipeLineException>(() => init.Buffer(1, 0));
            }
        }

        [Fact]
        public void CancelTest() {
            using (var cts = new CancellationTokenSource())
            using (var p = new PipeLine(cts.Token)) {
                p.Init(Enumerable.Range(0, 10), 1, 1, i => i);
                cts.Cancel();
                Assert.Throws<OperationCanceledException>(() => p.Start(null));
            }
        }

        [Fact]
        public void OnBeginTest()
        {
            using (var p = new PipeLine(CancellationToken.None))
            {
                var onBegin = false;
                var res = p.Init(Enumerable.Range(0, 100), 10, 10, i => i, () => onBegin = true).Out;
                p.Start(() => {
                    Assert.Equal(Enumerable.Range(0, 100).Sum(), res.Sum());
                });

                Assert.True(onBegin);
            }
        }

        [Fact]
        public void OnFinish()
        {
            using (var p = new PipeLine(CancellationToken.None))
            {
                var onFinish = false;
                p.Init(Enumerable.Range(0, 100), 10, 100, i => i, null, () => onFinish = true);
                p.Start(null);

                Assert.True(onFinish);
            }
        }

        [Fact]
        public void OnInterval()
        {
            using(var box = new BlockingCollection<int>())
            using (var p = new PipeLine(CancellationToken.None))
            {
                p.Init(Enumerable.Range(0, 100), 10, 100, i => i, null, null, o => box.Add((int)o));
                p.Start(null);
                Assert.Equal(Enumerable.Range(0, 100).Sum(), box.Sum());
            }
        }

        [Fact]
        public void OnInnerInterval() {
            using (var p = new PipeLine(CancellationToken.None))
            using (var box = new BlockingCollection<int>()) {
                p.InitSelectMany(Enumerable.Range(0, 10), 10, 100000, i => Enumerable.Repeat(i, 10), null, null, null,
                    o => box.Add((int) o));
                p.Start(null);
                box.CompleteAdding();
                Assert.Equal(Enumerable.Range(0,10).Sum()*10, box.GetConsumingEnumerable().Sum());
            }
        }
    }
}
