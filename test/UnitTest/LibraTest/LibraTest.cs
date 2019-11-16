using System;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.Libra;
using Ptolemy.Libra.Request;
using Ptolemy.SiMetricPrefix;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest.LibraTest {
    public class LibraTest {
        [Fact]
        public void OptionTest() {
            var args = "-E expressions -w 1000x2000 -e 1000 -W 1e9 /path/to/sqlite".Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var o = Parser.Default.ParseArguments<LibraOption>(args).MapResult(s => s, e => throw new Exception());

            Assert.Equal("expressions", o.Expressions);
            Assert.Equal("1000x2000", o.Sweep);
            Assert.Equal("1000", o.Seed);
            Assert.Equal("1e9", o.SweepStart);
            Assert.Equal("/path/to/sqlite", o.SqliteFile);
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
                opt.Seed = "xyz";
                Assert.Throws<FormatException>(() => opt.BuildRequest());

                opt.Seed = seed;

                var split = seed.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ParseLongWithSiPrefix()).ToArray();

                var (start, end) = split switch {
                    var x when x.Length == 2 => (split[0], split[1]),
                    var x when x.Length == 1 => (split[0], split[0]),
                    _ => throw new AssertFailedException()
                    };

                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());

                opt.SweepStart = "x";
                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());
                
                opt.SweepStart = "1";

                Assert.Throws<NullReferenceException>(() => opt.BuildRequest());
                opt.Sweep = "x";
                Assert.Throws<FormatException>(() => opt.BuildRequest());

                opt.Sweep = "100";

                var req = opt.BuildRequest();

                Assert.Equal(start, req.SeedStart);
                Assert.Equal(end, req.SeedEnd);
                Assert.Equal(opt.SqliteFile, req.SqliteFile);
            }
            finally {
                Directory.Delete(tmp, true);
            }
        }
        
    }
}