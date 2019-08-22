using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Ptolemy.Lupus.Record;
using Ptolemy.Parameters;

namespace UnitTest.VerbTest {
    public class LupusRequestTest {
        [Fact]
        public void ConstructorTest1() {
            Assert.NotNull(new LupusRequest());
        }

        [Fact]
        public void ConstructorTest2() {
            var e = new LupusRequest(0.6M, 0.046M, 1.0M, -0.6M, 0.046M, 1.0M, new[] {"a", "b", "c"});
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
            var e = new LupusRequest(new Transistor(0.6M, 0.046M, 1.0M), new Transistor(-0.6M, 0.046M, 1.0M),
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
            var json = new LupusRequest(0.6M, 0.046M, 1.0M, -0.6M, 0.046M, 1.0M, new[] {"a", "b", "c"}).ToJson();
            Assert.Equal(
                "{\"Vtn\":{\"Threshold\":0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"Vtp\":{\"Threshold\":-0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"FileList\":[\"a\",\"b\",\"c\"]}",
                json);
        }

        [Fact]
        public void FromJsonTest() {
            const string json =
                "{\"Vtn\":{\"Threshold\":0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"Vtp\":{\"Threshold\":-0.6,\"Sigma\":0.046,\"Deviation\":1.0},\"FileList\":[\"a\",\"b\",\"c\"]}";
            var e = LupusRequest.FromJson(json);
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
