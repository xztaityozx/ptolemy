//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using Ptolemy.Hydra.Request;
//using Ptolemy.Parameters;
//using Xunit;

//namespace UnitTest.ToolTest {
//    public class HydraRequestTest {
//        [Fact]
//        public void SweepSplitOptionTest() {
//            var data = new[] {
//                new{json="{\"SweepSplitOption\":0}",exp=SweepSplitOption.NoSplit},
//                new{json="{\"SweepSplitOption\":1}",exp=SweepSplitOption.SplitBySweep},
//                new{json="{\"SweepSplitOption\":2}",exp=SweepSplitOption.SplitBySeed}
//            };

//            foreach (var d in data) {
//                Assert.Equal(d.exp, HydraRequest.FromJson(d.json).SweepSplitOption);
//            }
//        }
//    }
//}
