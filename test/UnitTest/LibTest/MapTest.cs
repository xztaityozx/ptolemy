using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Ptolemy.Map;

namespace UnitTest.LibTest
{
    public class MapTest {
        [Fact]
        public void ConstructTest1() {
            var map = new Map<string, int>();
            Assert.NotNull(map);
            Assert.Equal(0, map["a"]);
        }

        [Fact]
        public void ConstructTest2() {
            var map = new Map<string, int>(30);
            Assert.NotNull(map);
            Assert.Equal(30, map["a"]);
        }

        [Fact]
        public void ConstructTest3() {
            var map = new Map<string, int>(() => 40);
            Assert.NotNull(map);
            Assert.Equal(40, map["a"]);
        }

        [Fact]
        public void ConstructTest4() {
            var map = new Map<int, List<int>>(() => new List<int>());
            Assert.NotNull(map);
            Assert.NotSame(map[1], map[2]);
        }

        [Fact]
        public void ToMapTest() {
            var map = Enumerable.Range(0, 100).ToMap(i => i, i => $"{i}");
            var expect = new Map<int, string>();
            foreach (var i in Enumerable.Range(0,100)) {
                expect[i] = $"{i}";
            }

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                expect.ToList(), map.ToList()
            );
        }
    }
}
