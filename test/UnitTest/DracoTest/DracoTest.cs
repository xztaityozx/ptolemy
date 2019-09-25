using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Draco;
using Ptolemy.Draco.Request;
using Ptolemy.FilePath;
using Ptolemy.Interface;
using Xunit;

namespace UnitTest.DracoTest {
    public class DracoTest {
        [Theory]
        [InlineData("--netlist net -w 10 -e 30 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net -w 10 --seed 40 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net -w 10 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net --sweep 20 -e 30 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net --sweep 20 --seed 40 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net --sweep 20 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net  -e 30 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net  --seed 40 /path/to/input /path/to/out", false)]
        [InlineData("--netlist net  /path/to/input /path/to/out", false)]
        [InlineData(" -w 10 -e 30 /path/to/input /path/to/out", true)]
        [InlineData(" -w 10 --seed 40 /path/to/input /path/to/out", true)]
        [InlineData(" -w 10 /path/to/input /path/to/out", true)]
        [InlineData(" --sweep 20 -e 30 /path/to/input /path/to/out", true)]
        [InlineData(" --sweep 20 --seed 40 /path/to/input /path/to/out", true)]
        [InlineData(" --sweep 20 /path/to/input /path/to/out", true)]
        [InlineData("  -e 30 /path/to/input /path/to/out", true)]
        [InlineData("  --seed 40 /path/to/input /path/to/out", true)]
        [InlineData("  /path/to/input /path/to/out", true)]
        public void ParseOptionTest(string input, bool throws) {
            var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (throws)
                Assert.Throws<Exception>(() =>
                    Parser.Default.ParseArguments<DracoOption>(args)
                        .MapResult(o => o.Build(), e => throw new Exception()));
            else {
                Parser.Default.ParseArguments<DracoOption>(args)
                    .MapResult(o => {
                        var t = o.Bind(null);
                        var opt = o.Build();
                        var outFile = Path.Combine(o.OutputDirectory, $"{t}_{o.NetList}");
                        Assert.Equal(o.Sweep, opt.Sweep);
                        Assert.Equal(o.Seed, opt.Seed);
                        Assert.Equal(o.BufferSize, opt.BufferSize);
                        Assert.Equal(FilePath.Expand(args[^2]), opt.InputFile);
                        Assert.Equal(FilePath.Expand(outFile), opt.OutputFile);
                        return 0;
                    }, e => throw new Exception());
            }
        }

        [Fact]
        public void RunTest() {
            var tmpDir = Path.Combine(Path.GetTempPath(), "Ptolemy.Draco_Test");
            try {
                Directory.CreateDirectory(tmpDir);
                var input = Path.Combine(tmpDir, "input");
                var output = Path.Combine(tmpDir, "output");

                using (var sw = new StreamWriter(input)) {
                    sw.WriteLine(@" time         voltage    voltage    voltage    voltage
             n1         n2         blb        bl
    0.        800.0000m    0.         0.         0.
  100.00000p  799.9898m    1.1658u  799.0462m  799.0473m
  200.00000p  799.9892m    1.1863u  799.8032m  799.8044m
  300.00000p  799.9889m    1.1876u  799.8091m  799.8102m
  400.00000p  799.9888m    1.1883u  799.8191m  799.8202m
  500.00000p  799.9888m    1.1890u  799.8159m  799.8170m
  600.00000p  799.9889m    1.1896u  799.8127m  799.8139m
  700.00000p  799.9889m    1.1903u  799.8096m  799.8107m");
                    sw.Flush();
                }

                var req = new DracoRequest {
                    BufferSize = 10000,
                    Sweep = 1,
                    Seed = 1,
                    InputFile = input,
                    OutputFile = output
                };

                using var draco = new Draco(CancellationToken.None, req);
                draco.Run();
            }
            finally {
                Directory.Delete(tmpDir, true);
            }
        }
    }
}
