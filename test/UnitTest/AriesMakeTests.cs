using System;
using System.IO;
using System.Threading;
using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptolemy.Aries;
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
    }
}