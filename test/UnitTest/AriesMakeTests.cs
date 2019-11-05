using System;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.Aries;
using Ptolemy.SiMetricPrefix;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTest {
    public class AriesMakeTests {
        [Theory]
        [InlineData("-W seed:100", false)]
        [InlineData("-W sweep:10", false)]
        public void ParseTest(string arg, bool throws) {
            var file = Path.GetTempFileName();
            using (var sw = new StreamWriter(file)) sw.WriteLine("file");
            var args = (arg + " " + file).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (throws) {
                Assert.Throws<AriesException>(() => Parser.Default.ParseArguments<AriesMake>(args));
            }
            else {
                var res = Parser.Default.ParseArguments<AriesMake>(args)
                    .MapResult(o => o, e => throw new AssertFailedException());
                Assert.Equal(arg.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], res.SplitOption);
            }
        }
        [Theory]
        [InlineData("0,100p,20n","4n", "4n")]
        [InlineData("0,100p,20n", "4n,8n", "4n 8n")]
        [InlineData("0,1,10", "all", "0 1 2 3 4 5 6 7 8 9 10")]
        [InlineData("0,0,0", "0:1:10", "0 1 2 3 4 5 6 7 8 9 10")]
        [InlineData("0,0,0", "4n,8n,0:1:10", "4n 8n 0 1 2 3 4 5 6 7 8 9 10")]
        public void PlotTimeTest(string timeRange,string args, string expect) {

            var e = expect.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ParseDecimalWithSiPrefix())
                .ToArray();

            CollectionAssert.AreEquivalent(e, AriesMake.GeneratePlotTimeEnumerable(args, timeRange).ToArray());
        }
    }
}