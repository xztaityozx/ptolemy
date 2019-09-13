using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ptolemy.Lupus;
using Ptolemy.SiMetricPrefix;
using Xunit;

namespace UnitTest.LupusTest {
    public class LupusOptionTest {
        [Theory]
        [InlineData("wave","/target", "/result", "a,b,c,d", "1,2,3", false)]
        [InlineData("w","/target", "/result", "a", "1n,2n,3n", false)]
        [InlineData("v","/target", "/result", "", "", true)]
        public void OptionBuildRequest(string wv,string td, string rf,string sig,string pp, bool th) {
            var opt = new Options {
                TargetDirectory = td,
                ResultFileName = rf,
                Signals = sig.Split(',',StringSplitOptions.RemoveEmptyEntries).ToList(),
                PlotPointString = pp,
                WaveView = wv
            };

            if(th) Assert.Throws<LupusException>(opt.BuildLupusResult);
            else {
                var req = opt.BuildLupusResult();
                Assert.Equal(td, req.TargetDirectory);
                Assert.Equal(wv, req.WaveViewPath);
                Assert.Equal(rf, req.ResultFileName);
                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    sig.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    req.Signals
                );
                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    pp.Split(',').Select(SiMetricPrefix.ParseDecimalWithSiPrefix).ToArray(),
                    new[] {req.PlotPoint.Start, req.PlotPoint.Step,req.PlotPoint.Stop}
                );
            }
        }
    }
}
