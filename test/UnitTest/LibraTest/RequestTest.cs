using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ptolemy.Libra.Request;
using Xunit;
using Ptolemy.Map;

namespace UnitTest.LibraTest {
    public class RequestTest {
        [Theory]
        [InlineData("A", 10)]
        [InlineData("B", 10000)]
        [InlineData("C", 22222)]
        public void GetKeyTest(string signal, double time) {
            var t = (decimal) time;
            Assert.Equal($"{signal}/{t:E5}", LibraRequest.GetKey(signal, t));
        }

        [Fact]
        public void ConstructorTest() {
            const string exp = "A[1]<10, 100>=B[30], 1p != C[30n]&&A[1]<10";
            var actual = new LibraRequest(exp, (1, 2), (3, 4), "path");
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] {"exp1", "exp2", "exp3 && exp1"}, actual.Expressions
            );

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new Map<string, string> { ["exp1"]="A[1]<10",["exp2"]="100>=B[30]",["exp3"]="1p!=C[30n]"}, actual.Conditions
            );
        }

        [Fact]
        public void BuildFilterTest() {
            var req = new Ptolemy.Libra.Request.LibraRequest {
                Conditions = new Dictionary<string, string> {
                    ["A"] = "x[1] <= 20",["B"] = "x[2] >= 30",
                    ["C"] = "x[3] > 40",["D"] = "x[4] < 50",
                    ["E"] = "x[5] == 60",["F"] = "x[6] != 70",
                    ["a"] = "x[1] <= y[1]",["b"] = "x[2] >= y[2]",
                    ["c"] = "x[3] < y[3]",["d"] = "x[4] > y[4]",
                    ["e"] = "x[5] == y[5]",["f"] = "x[6] != y[6]"
                },
                Expressions = new List<string> {
                    "a", "b", "c", "d", "e", "f", "A", "B", "C", "D", "E", "F",
                    "!a", "!b", "!c", "!d", "!e", "!f", "!A", "!B", "!C", "!D", "!E", "!F",
                    "A && D",
                    "c || B",
                    "(e && f) && (F || a)",
                    "!(b && E)",
                    "!d || C"
                }
            };

            var map = new Map<string, decimal> {
                [$"x/{1M:E5}"] = 19,
                [$"x/{2M:E5}"] = 30,
                [$"x/{3M:E5}"] = 41,
                [$"x/{4M:E5}"] = 49,
                [$"x/{5M:E5}"] = 60,
                [$"x/{6M:E5}"] = 1,

                [$"y/{1M:E5}"] = 18,
                [$"y/{2M:E5}"] = 31,
                [$"y/{3M:E5}"] = 40,
                [$"y/{4M:E5}"] = 50,
                [$"y/{5M:E5}"] = 20,
                [$"y/{6M:E5}"] = 1,
            };

            var d = req.BuildFilter();
            foreach (var expected in new[] {
                false, false, false, false, false, false, true, true, true, true, true, true,
                true, true, true, true, true, true, false, false, false, false, false, false,
                true,
                true,
                false,
                true,
                true
            }.Select((s, i) => new {s, i})) {
                Assert.Equal(expected.s, d[expected.i](map));
            }

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] {"x", "y"}, req.SignalList.ToList());

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[]{1M,2M,3M,4M,5M,6M}, req.TimeList.ToList());
        }
    }
}
