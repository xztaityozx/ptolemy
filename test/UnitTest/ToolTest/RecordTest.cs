using Xunit;

namespace UnitTest.ToolTest {
    public class RecordTest {
        [Fact]
        public void ConstructorTest() {
            Assert.NotNull(new Ptolemy.Lupus.Record.Record());
            var actual = new Ptolemy.Lupus.Record.Record(10, 20, "signal", 30, 40);
            Assert.Equal(10, actual.Sweep);
            Assert.Equal(20, actual.Seed);
            Assert.Equal("signal/3.0000000000E+001", actual.Key);
            Assert.Equal(40, actual.Value);
        }

        [Fact]
        public void EncodeKeyTest() {
            var data = new[] {
                new{s="signal",t=30M},
                new{s="A",t=1M},
                new{s="C", t=-1M}
            };
            foreach (var d in data) {
                Assert.Equal($"{d.s}/{d.t:E10}", Ptolemy.Lupus.Record.Record.EncodeKey(d.s, d.t));
            }
        }

        [Fact]
        public void ToStringTest() {
            const long w = 10;
            const long e = 20;
            const decimal t = 40M;
            const decimal v = 50M;
            const string s = "signal";
            Assert.Equal($"Signal/Time:{Ptolemy.Lupus.Record.Record.EncodeKey(s, t)}, Sweep:{w}, Value:{v}, Seed:{e}",
                new Ptolemy.Lupus.Record.Record(w, e, s, t, v).ToString());
        }
    }
}
