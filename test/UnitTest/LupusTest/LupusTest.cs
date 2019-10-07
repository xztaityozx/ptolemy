using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CommandLine;
using Ptolemy.Lupus;
using Xunit;

namespace UnitTest.LupusTest {
    using CAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
    public class LupusTest {
        [Fact]
        public void BuildRequestsTest2() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
            Directory.CreateDirectory(tmp);
            var files = new List<string>();

            for (var i = 0; i < 10; i++) {
                var path = Path.Combine(tmp, $"{i}");
                files.Add(path);
                using var sw = new StreamWriter(path);
                sw.WriteLine($"{i}");
            }

            try {
                var opt = new LupusOptions {
                    Expressions = "A[1]<999,B[2]>=1000",
                    Files = files
                };

                var req = opt.BuildRequest();
                Assert.Equal(10, req.DracoRequests.Length);
                CAssert.AreEquivalent(Enumerable.Range(0, 10).Select(s => (long) s).ToList(),
                    req.DracoRequests.Select(s => s.Sweep).ToList());
                CAssert.AreEquivalent(files, req.DracoRequests.Select(s => s.InputFile).ToList());
                Assert.All(req.DracoRequests.Select(s => s.OutputFile),
                    s => Assert.Equal(req.LibraRequest.SqliteFile, s));
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }
        [Fact]
        public void BuildRequestsTest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
            Directory.CreateDirectory(tmp);
            var files = new List<string>();

            for (var i = 0; i < 10; i++) {
                var path = Path.Combine(tmp, $"{i}");
                files.Add(path);
                using var sw = new StreamWriter(path);
                sw.WriteLine($"{i}");
            }

            try {
                var opt = new LupusOptions {
                    Expressions = "A[1]<999,B[2]>=1000",
                    TargetDirectory = tmp
                };

                var req = opt.BuildRequest();
                Assert.Equal(10, req.DracoRequests.Length);
                CAssert.AreEquivalent(Enumerable.Range(0, 10).Select(s => (long)s).ToList(), req.DracoRequests.Select(s => s.Sweep).ToList());
                CAssert.AreEquivalent(files, req.DracoRequests.Select(s => s.InputFile).ToList());
                Assert.All(req.DracoRequests.Select(s => s.OutputFile),
                    s => Assert.Equal(req.LibraRequest.SqliteFile, s));
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void ThrowsTest() {
            Assert.Throws<LupusException>(() => new LupusOptions().BuildRequest());
        }
    }
}