using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.LibTest
{
    public class RangeTest
    {
        [Fact]
        public void ConstructorTest() {
            var data = new[] {
                new {start = 1, step = 2, stop = 100},
                new {start = 5, step = 3, stop = 100}
            };
            foreach (var d in data) {
                var first = new Range(d.start, d.step, d.stop);
                Assert.Equal(d.start, first.Start);
                Assert.Equal(d.step, first.Step);
                Assert.Equal(d.stop, first.Stop);

                var expected = new List<decimal>();
                for(var i=d.start;i<=d.stop;i+=d.step) expected.Add(i);

                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    expected,
                    first.ToEnumerable().ToList()
                );


                var second = new Range((d.start, d.step, d.stop));
                Assert.Equal(d.start, second.Start);
                Assert.Equal(d.step, second.Step);
                Assert.Equal(d.stop, second.Stop);

                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    expected,
                    second.ToEnumerable().ToList()
                );
            }
        }
        
    }
}
