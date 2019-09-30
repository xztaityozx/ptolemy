using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Libra;
using Ptolemy.Libra.Request;
using Ptolemy.Map;
using Ptolemy.OptionException;
using Ptolemy.Repository;
using Xunit;

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
                CancellationToken.None).Zip(req.SignalList, (l,s)=>Tuple.Create(s,l)).ToList();

            var libra = new Libra(CancellationToken.None, req);
            var actual = libra.Run();

            CAssert.AreEquivalent(expect, actual);

            Directory.Delete(tmp, true);
        }

        [Theory]
        [InlineData("", true, new[]{""},new[]{""}, "")]
        [InlineData("-e 1,2 -E A[10]<100 sqlite", false, new[]{"A[10]<100"}, new[]{"exp1"}, "sqlite")]
        [InlineData("-e 1,2 -w 10,20 -E B[1p]<99,C[9n]>=5 sqlite", false, new[]{"B[1p]<99","C[9n]>=5"},new[]{"exp1","exp2"}, "sqlite")]
        public void LibraOptionBuildRequestTest(string commandLine, bool throws, string[] conditions, string[] expressions, string sqlite) {
            var args = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (throws) {
                Assert.Throws<ParseFailedException>(() => Parser.Default.ParseArguments<LibraOption>(args)
                    .MapResult(o => o.BuildRequest(), e => throw new ParseFailedException()));
            }
            else {
                var o = Parser.Default.ParseArguments<LibraOption>(args)
                    .MapResult(x => x, e => throw new ParseFailedException());

                var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Libra.Test.BuildRequestTest");
                var db = Path.Combine(tmp, sqlite);
                Directory.CreateDirectory(tmp);
                using (File.Create(db)) { }

                o.SqliteFile = db;

                var req = o.BuildRequest();

                CAssert.AreEquivalent(
                    conditions.Select((s, i) => new {s, i = i + 1}).ToMap(k => $"exp{k.i}", k => k.s),
                    req.Conditions
                );

                CAssert.AreEquivalent(
                    expressions, req.Expressions
                );

                Assert.Equal(db, req.SqliteFile);
                Directory.Delete(tmp, true);
            }
        }
    }
}