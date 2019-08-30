using System.Linq;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.LibTest {
    public class TransistorTest {
        [Fact]
        public void ConstructorTest1() {
            var (t, s, d) = (0.6, 0.046, 1.0);
            var transistor = new Transistor(t, s, d);
            Assert.Equal((decimal)t, transistor.Threshold);
            Assert.Equal((decimal)s, transistor.Sigma);
            Assert.Equal((decimal) d, transistor.Deviation);
        }

        [Fact]
        public void ConstructorTest2() {
            var (t, s, d) = (0.6M, 0.046M, 1.0M);
            var transistor = new Transistor(t, s, d);
            Assert.Equal(t, transistor.Threshold);
            Assert.Equal(s, transistor.Sigma);
            Assert.Equal(d, transistor.Deviation);
        }
        [Fact]
        public void ConstructorTest3() {
            var (t, s, d) = (0.6M, 0.046M, 1.0M);
            var transistor = new Transistor((t, s, d));
            Assert.Equal(t, transistor.Threshold);
            Assert.Equal(s, transistor.Sigma);
            Assert.Equal(d, transistor.Deviation);
        }

        [Fact]
        public void GetParametersString() {
            var t = new Transistor(0.6M, 0.046M, 1.0M);
            var expect = new[] {
                $"Threshold: {0.6M}",
                $"Sigma: {0.046M}",
                $"Deviation: {1.0M}"
            };
            
            foreach (var item in t.GetParameterStrings().Zip(expect, (s, k) => new{s,k} )) {
                Assert.Equal(item.k, item.s);
            }
        }

        [Fact]
        public void ToStringTest() {
            var expect = $"t_{0.6M:E10}_s_{0.046M:E10}_d_{1.0M:E10}";
            var actual = new Transistor(0.6M, 0.046M, 1.0M).ToString();
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void ToTableNameTest() {
            var vtn=new Transistor(0.6,0.046,1.0);
            var vtp = new Transistor(-0.6, 0.046, 1.0);
            var expect = $"vtn_{vtn}_vtp_{vtp}";
            var actual = Transistor.ToTableName(vtn, vtp);
            Assert.Equal(expect,actual);
        }
    }
}
