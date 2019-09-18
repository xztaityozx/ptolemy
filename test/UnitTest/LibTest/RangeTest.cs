using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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
                
                var third = new Range($"{d.start},{d.step},{d.stop}");
                Assert.Equal(d.start, third.Start);
                Assert.Equal(d.step, third.Step);
                Assert.Equal(d.stop, third.Stop);
                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    expected,
                    second.ToEnumerable().ToList()
                );
                
                var fourth = new Range($"{d.start},{d.stop}");
                Assert.Equal(d.start, fourth.Start);
                Assert.Equal(1, fourth.Step);
                Assert.Equal(d.stop, fourth.Stop);
                
                var fifth = new Range($"{d.start}");
                Assert.Equal(d.start, fifth.Start);
                Assert.Equal(1, fifth.Step);
                Assert.Equal(d.start, fifth.Stop);
            }
        }

        [Theory]
        [InlineData(",,", 1,2,100, "1,2,100")]
        [InlineData("10,20,40", 1,2,3, "10,20,40")]
        [InlineData("1,10", 10,2,3, "1,1,10")]
        [InlineData("1,,4", 10,2,3, "1,2,4")]
        public void Bind(string input, double start, double step, double stop, string ex) {
            var actual = new Range(input, ((decimal)start, (decimal)step, (decimal)stop));
            var expect = new Range(ex);

            Assert.Equal(JsonConvert.SerializeObject(expect), JsonConvert.SerializeObject(actual));
        }
        
    }
}
