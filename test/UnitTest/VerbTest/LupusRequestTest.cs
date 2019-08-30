using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Ptolemy.Lupus.Record;
using Ptolemy.Parameters;

namespace UnitTest.VerbTest {

    public class LupusGetRequestTest {
        [Fact]
        public void GetSigmaEnumerableTest() {
            var a = new LupusGetRequest(0M, 0M, 0M, 0M, null, (1M, 1M, 100M), (0, 0, 0), (0, 0, 0)).GetSigmaEnumerable()
                .ToArray();
            var e = Enumerable.Range(1, 100).Select(x=>(decimal)x).ToArray();
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                e, a
            );
        }

        [Fact]
        public void GetTransistorEnumerable() {
            var a = new LupusGetRequest(1M, 2M, 3M, 4M, null, (1M, 1M, 100M), (0, 0, 0), (0, 0, 0))
                .GetTransistorEnumerable()
                .Select(x=>$"{x.Item1}{x.Item2}")
                .ToArray();
            var e = Enumerable.Range(1, 100).Select(x => (new Transistor(1M, x, 2M), new Transistor(3M, x, 4M)))
                .Select(x=>$"{x.Item1}{x.Item2}")
                .ToArray();

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(e, a);
        }

        [Fact]
        public void ConstructorTest1() {
            Assert.NotNull(new LupusGetRequest());
        }

        [Fact]
        public void ConstructorTest2()
        {
            var e = new LupusGetRequest(0.6M, 0.046M, 1.0M, -0.6M, 0.046M, 1.0M, null, (1, 2, 3), (4, 5, 6));
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0.046M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0.046M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Assert.Equal(1, e.SweepStart);
            Assert.Equal(2, e.SweepStep);
            Assert.Equal(3, e.SweepEnd);
            Assert.Equal(4, e.SeedStart);
            Assert.Equal(5, e.SeedStep);
            Assert.Equal(6, e.SeedEnd);
            Assert.Equal(LupusGetRequest.RequestMode.Single, e.Mode);
        }

        [Fact]
        public void ConstructorTest3() {
            var e = new LupusGetRequest(0.6M, 1.0M, -0.6M, 1.0M, null, (1, 2, 3), (4, 5, 6), (7, 8, 9));
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Assert.Equal(1, e.SigmaStart);
            Assert.Equal(2, e.SigmaStep);
            Assert.Equal(3, e.SigmaEnd);
            Assert.Equal(4, e.SweepStart);
            Assert.Equal(5, e.SweepStep);
            Assert.Equal(6, e.SweepEnd);
            Assert.Equal(7, e.SeedStart);
            Assert.Equal(8, e.SeedStep);
            Assert.Equal(9, e.SeedEnd);
            Assert.Equal(LupusGetRequest.RequestMode.Range, e.Mode);
        }
        [Fact]
        public void ConstructorTest4()
        {
            var e = new LupusGetRequest(new Transistor(0.6M, 0, 1.0M), new Transistor(-0.6M, 0, 1.0M), null, (1, 2, 3), (4, 5, 6), (7, 8, 9));
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Assert.Equal(1, e.SigmaStart);
            Assert.Equal(2, e.SigmaStep);
            Assert.Equal(3, e.SigmaEnd);
            Assert.Equal(4, e.SweepStart);
            Assert.Equal(5, e.SweepStep);
            Assert.Equal(6, e.SweepEnd);
            Assert.Equal(7, e.SeedStart);
            Assert.Equal(8, e.SeedStep);
            Assert.Equal(9, e.SeedEnd);
            Assert.Equal(LupusGetRequest.RequestMode.Range, e.Mode);
        }
        [Fact]
        public void ConstructorTest5()
        {
            var e = new LupusGetRequest(new Transistor(0.6M, 0.046M, 1.0M), new Transistor(-0.6M, 0.046M, 1.0M), null, (4, 5, 6), (7, 8, 9));
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0.046M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0.046M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Assert.Equal(4, e.SweepStart);
            Assert.Equal(5, e.SweepStep);
            Assert.Equal(6, e.SweepEnd);
            Assert.Equal(7, e.SeedStart);
            Assert.Equal(8, e.SeedStep);
            Assert.Equal(9, e.SeedEnd);
            Assert.Equal(LupusGetRequest.RequestMode.Single, e.Mode);
        }
    }

    public class LupusPushRequestTest {
        [Fact]
        public void ConstructorTest1() {
            Assert.NotNull(new LupusPushRequest());
        }

        [Fact]
        public void ConstructorTest2() {
            var e = new LupusPushRequest(0.6M, 0.046M, 1.0M, -0.6M, 0.046M, 1.0M, new[] {"a", "b", "c"});
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0.046M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0.046M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(new[] {"a", "b", "c"},
                e.FileList.ToList());
        }

        [Fact]
        public void ConstructorTest3() {
            var e = new LupusPushRequest(new Transistor(0.6M, 0.046M, 1.0M), new Transistor(-0.6M, 0.046M, 1.0M),
                new[] {"a", "b", "c"});
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0.046M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0.046M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(new[] {"a", "b", "c"},
                e.FileList.ToList());
        }

        [Fact]
        public void ToJsonTest() {
            var json = new LupusPushRequest(0.6M, 0.046M, 1.0M, -0.6M, 0.046M, 1.0M, new[] {"a", "b", "c"}).ToJson();
            Assert.Equal(
                "{\"FileList\":[\"a\",\"b\",\"c\"],\"Vtn\":{\"Threshold\":0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"Vtp\":{\"Threshold\":-0.6,\"Sigma\":0.046,\"Deviation\":1.0}}",
                json);
        }

        [Fact]
        public void FromJsonTest() {
            const string json =
                "{\"Vtn\":{\"Threshold\":0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"Vtp\":{\"Threshold\":-0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"FileList\":[\"a\",\"b\",\"c\"]}";
            var e = LupusPushRequest.FromJson(json);
            Assert.Equal(0.6M, e.Vtn.Threshold);
            Assert.Equal(0.046M, e.Vtn.Sigma);
            Assert.Equal(1.0M, e.Vtn.Deviation);
            Assert.Equal(-0.6M, e.Vtp.Threshold);
            Assert.Equal(0.046M, e.Vtp.Sigma);
            Assert.Equal(1.0M, e.Vtp.Deviation);
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(new[] {"a", "b", "c"},
                e.FileList.ToList());
        }
    }
}
