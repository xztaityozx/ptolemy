using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.Libra;
using Ptolemy.Libra.Request;
using Ptolemy.Repository;
using Ptolemy.SiMetricPrefix;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest.LibraTest {
    using CAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
    public class LibraTest {
        [Fact]
        public void RunTest() {
            var box = new List<ResultEntity>();
            var r = new Random(DateTime.Now.Millisecond);

            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Libra.LibraTest.RunTest");
            Directory.CreateDirectory(tmp);
            var sqlite = Path.Combine(tmp, "sqlite");
            for(var i =0;i<100;i++) box.Add(new ResultEntity {
                Sweep = i, Seed = i % 3, Signal = i%2==0?"A":"B", Time = i%5,Value = r.Next()
            });

            var req = new LibraRequest {
                SweepStart = 20, SweepEnd = 50, SeedEnd = 2, SeedStart = 1, SqliteFile = sqlite,
                Conditions = new Dictionary<string, string> {
                    ["a"] = $"A[1]<{r.Next()}", ["b"] = $"A[2]<={r.Next()}",
                    ["c"] = $"B[3]>={r.Next()}", ["d"] = $"B[4]>A[1]"
                },
                Expressions = new List<string> {
                    "a", "b&&c", "a||d"
                }
            };

            var delegates = req.BuildFilter();

            using var repo = new SqliteRepository(sqlite);
            repo.BulkUpsert(box);
            var expect = repo.Aggregate(req.SignalList, (1, 2), (20, 50), delegates, LibraRequest.GetKey,
                CancellationToken.None).Zip(req.Expressions.Select(s => 
                    req.Conditions.Aggregate(s, (exp, x) => exp.Replace(x.Key,x.Value))
                ), (l,s)=>Tuple.Create(s,l)).ToList();

            var libra = new Libra(CancellationToken.None);
            var actual = libra.Run(req);

            CAssert.AreEquivalent(expect, actual);

            Directory.Delete(tmp, true);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1,30")]
        public void LibraOptionBuildRequestTest(string seed) {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Libra.LibraOptionBuildRequestTest");
            Directory.CreateDirectory(tmp);
            try {
                var opt = new LibraOption();

                Assert.Throws<LibraException>(() => opt.BuildRequest());
                opt.Expressions = "a[1]<=10,b[2]>=20,c[3]!=c[4],a[5] != 30 && b[6] >= c[7]";
                Assert.Throws<LibraException>(() => opt.BuildRequest());
                opt.SqliteFile = Path.Combine(tmp, "sqlite");
                using (var sw = new StreamWriter(opt.SqliteFile)) sw.WriteLine("dummy sqlite");

                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());
                opt.SeedString = "xyz";
                Assert.Throws<FormatException>(() => opt.BuildRequest());

                opt.SeedString = seed;

                var split = seed.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ParseLongWithSiPrefix()).ToArray();

                var (start, end) = split switch {
                    var x when x.Length == 2 => (split[0], split[1]),
                    var x when x.Length == 1 => (split[0], split[0]),
                    _ => throw new AssertFailedException()
                    };

                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());

                opt.SweepStartString = "x";
                Assert.Throws<FormatException>(() => opt.BuildRequest());
                
                opt.SweepStartString = "1";

                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());
                opt.SweepString = "x";
                Assert.Throws<FormatException>(() => opt.BuildRequest());

                opt.SweepString = "100";

                var req = opt.BuildRequest();

                Assert.Equal(start, req.SeedStart);
                Assert.Equal(end, req.SeedEnd);
                Assert.Equal(1L, req.SweepStart);
                Assert.Equal(100L, req.SweepEnd);
                Assert.Equal(opt.SqliteFile, req.SqliteFile);
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }
        
    }
}