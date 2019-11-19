using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ptolemy.Map;
using Ptolemy.Repository;
using Xunit;

namespace UnitTest.RepositoryTest {
    public class HydraRepositoryTest {
        public static IEnumerable<ParameterEntity> GenerateDummyParameters() {
            yield return new ParameterEntity {
                Vtn = "vtn", Vtp = "vtp", NetList = "netlist", Signals = "a:b:c", Time = "1:2:3",
                Includes = "include1:include2", Vdd = 0.8M, Temperature = 25.0M, Hspice = "hspice",
                Gnd = 0M, IcCommand = "command1:command2:command3", HspiceOption = "option1:option2"
            };
        }

        [Fact]
        public void AddDbTest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Repository", "Test");
            Directory.CreateDirectory(tmp);

            var root = Path.Combine(tmp, "dbRoot");
            Directory.CreateDirectory(root);

            var pe = GenerateDummyParameters().First();

            try {
                using var hub = new DbHub(CancellationToken.None, root, 10, null);
                var key = hub.AddDb(pe);
                Assert.Equal(pe.Hash(), key);

                hub.CloseDb(pe);

                key = hub.AddDb(pe);
                Assert.Equal(pe.Hash(), key);
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }

        [Fact]
        public void AddEntityTest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Repository", "Test");
            Directory.CreateDirectory(tmp);

            var root = Path.Combine(tmp, "dbRoot");
            Directory.CreateDirectory(root);

            var pe = GenerateDummyParameters().First();

            try {
                using var hub = new DbHub(CancellationToken.None, root, 1, null);
                var key = hub.AddDb(pe);

                hub.AddEntity(key, new ResultEntity {
                    Sweep = 1, Seed = 2,
                    Signal = "signal", Time = 0,
                    Value = 1
                });

                hub.CloseDb(key);

                //var repo = new ReadOnlyRepository(Path.Combine(root, $"{key}.sqlite"));
                //Func<Map<string, decimal>, bool> f = m => m[$"signal/{0M:E5}"] == 1;
                //var res = repo.Aggregate(
                //    CancellationToken.None, new[] {"signal"}, new[] {f}, 2, new (long start, long end)[] {(1, 10)},
                //    (s, d) => $"{s}/{d:E5}"
                //).First();

                //Assert.Equal(1, res);
            }
            finally {
                //Directory.Delete(tmp, true);
            }
        }
    }
}
