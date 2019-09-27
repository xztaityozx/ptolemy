using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ptolemy.Map;
using Ptolemy.Repository;
using Xunit;
using Ptolemy.Libra.Request;

namespace UnitTest.RepositoryTest {
    public class RepositoryTest {
        [Fact]
        public void AggregateTest() {
            var entities = new List<ResultEntity>();
            var r = new Random(DateTime.Now.Millisecond);
            for (var i = 0; i < 100; i++) {
                entities.Add(new ResultEntity {
                    Sweep = i + 1,
                    Seed = i % 3,
                    Signal = i % 2 == 0 ? "A" : "B",
                    Time = i % 2,
                    Value = r.Next()
                });
            }

            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lib.RepositoryTest_AggregateTest");
            var sqlite = Path.Combine(tmp, "sqlite.db");
            Directory.CreateDirectory(tmp);


            using var repo = new SqliteRepository(sqlite);
            repo.BulkUpsert(entities);

            var ds = new List<Func<Map<string, decimal>, bool>> {
                m => m[LibraRequest.GetKey("A", 1)] <= r.Next(),
                m => m[LibraRequest.GetKey("B", 0)] >= r.Next(),
                m => m[LibraRequest.GetKey("A", 1)] > r.Next(),
                m => m[LibraRequest.GetKey("B", 0)] < r.Next()
            };

            var filtered = entities.Where(s => s.Signal == "A" || s.Signal == "B")
                .Where(s => 1 <= s.Seed && s.Seed <= 2)
                .Where(s => 20 <= s.Sweep && s.Sweep <= 50)
                .GroupBy(x => new {x.Sweep, x.Seed})
                .Select(g => g.ToMap(k => LibraRequest.GetKey(k.Signal, k.Time), v => v.Value))
                .ToList();


            var expect = new long[ds.Count];
            foreach (var item in ds.Select((d, i) => new {d, i})) {
                expect[item.i] = filtered.Count(item.d);
            }

            var actual = repo.Aggregate(new List<string> {"A", "B"}, (1, 2), (20, 50), ds, LibraRequest.GetKey);

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                expect, actual
            );

            Directory.Delete(tmp, true);
        }
    }
}