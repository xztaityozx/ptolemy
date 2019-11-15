using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.Libra.Request;
using Ptolemy.SiMetricPrefix;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest.LibraTest {
    public class SweepsTest {
        [Theory]
        [InlineData("1x300", 1L)]
        [InlineData("2x4000", 10L)]
        [InlineData("1000", 1000L)]
        public void NewTest(string arg, long start) {
            var sweep = new Sweeps(arg, start);

            var split = arg.Split('x', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ParseLongWithSiPrefix())
                .ToList();

            if (split.Count == 2) {
                Assert.Equal(split[0], sweep.Times);
                Assert.Equal(split[1], sweep.Size);
                Assert.Equal(split[0]*split[1], sweep.Total);
                Assert.Equal(start, sweep.Start);
            }
            else {
                Assert.Equal(1, sweep.Times);
                Assert.Equal(split[0], sweep.Size);
                Assert.Equal(split[0], sweep.Total);
                Assert.Equal(start, sweep.Start);
            }
        }

        [Fact]
        public void RepeatTest() {
            var sweep = new Sweeps("100x200", 1);
            CollectionAssert.AreEquivalent(
                Enumerable.Repeat(200L, 100).ToList(), sweep.Repeat().ToList()
            );
        }

        [Fact]
        public void SectionTest() {
            var sweep = new Sweeps("100x200", 1);
            var box = new List<(long start,long end)>();
            {
                var s = 1L;
                for (var i = 0; i < 100; i++) {
                    box.Add((s, s + 200 - 1));
                    s += 200;
                }
            }
            foreach (var x in box.Zip(sweep.Section(), (f,s)=>new{f,s})) {
                Assert.Equal(x.s.start, x.f.start);
                Assert.Equal(x.s.end, x.f.end);
            }
        }
    }
}
