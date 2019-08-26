using System;
using System.Collections.Concurrent;
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
                var s = p.Start(() => { Assert.Equal(4500, x.Out.Sum()); });
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }

        [Fact]
        public void InitSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var x = p.InitSelectMany(Enumerable.Range(0, 100), 10, 10, i => Enumerable.Repeat(i, 3));
                Assert.NotNull(x);
                Assert.Null(x.Next);
                var s = p.Start(() => Assert.Equal(4500 * 3, x.Out.Sum()));
                Assert.Equal(PipeLine.PipeLineStatus.Completed, s);
            }
        }
    }
}
