using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ptolemy.Interface;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;
using Xunit;

namespace UnitTest.InterfaceTest {
    public class PtolemyTransistorOptionTest {
        [Theory]
        [InlineData(",,", ",,", ",,", ",,", null, "0.6,0.046,1.0", "-0.6,0.046,1.0")]
        [InlineData(",,", ",,", "1,2,3", ",,", null, "1,2,3", "-0.6,0.046,1.0")]
        [InlineData(",,", ",,", ",,", "1,2,3", null, "0.6,0.046,1.0", "1,2,3")]
        [InlineData(",,", ",,", "1,2,3", "4,5,6", null, "1,2,3", "4,5,6")]
        [InlineData(",,", ",,", ",,", ",,", 8, "0.6,8,1.0", "-0.6,8,1.0")]
        [InlineData(",,", ",,", "1,2,3", ",,", 8, "1,8,3", "-0.6,8,1.0")]
        [InlineData(",,", ",,", ",,", "1,2,3", 8, "0.6,8,1.0", "1,8,3")]
        [InlineData(",,", ",,", "1,2,3", "4,5,6", 8, "1,8,3", "4,8,6")]
        [InlineData("10,20,30", ",,", ",,", ",,", null, "10,20,30", "-0.6,0.046,1.0")]
        [InlineData("10,20,30", ",,", "1,2,3", ",,", null, "10,20,30", "-0.6,0.046,1.0")]
        [InlineData("10,20,30", ",,", ",,", "1,2,3", null, "10,20,30", "1,2,3")]
        [InlineData("10,20,30", ",,", "1,2,3", "4,5,6", null, "10,20,30", "4,5,6")]
        [InlineData(",,", "10,20,30", ",,", ",,", null, "0.6,0.046,1.0", "10,20,30")]
        [InlineData(",,", "10,20,30", "1,2,3", ",,", null, "1,2,3", "10,20,30")]
        [InlineData(",,", "10,20,30", ",,", "1,2,3", null, "0.6,0.046,1.0", "10,20,30")]
        [InlineData(",,", "10,20,30", "1,2,3", "4,5,6", null, "1,2,3", "10,20,30")]
        [InlineData("10,20,30", "40,50,60", ",,", ",,", null, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", "1,2,3", ",,", null, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", ",,", "1,2,3", null, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", "1,2,3", "4,5,6", null, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", ",,", ",,", ",,", 100, "10,20,30", "-0.6,100,1.0")]
        [InlineData("10,20,30", ",,", "1,2,3", ",,", 100, "10,20,30", "-0.6,100,1.0")]
        [InlineData("10,20,30", ",,", ",,", "1,2,3", 100, "10,20,30", "1,100,3")]
        [InlineData("10,20,30", ",,", "1,2,3", "4,5,6", 100, "10,20,30", "4,100,6")]
        [InlineData(",,", "10,20,30", ",,", ",,", 100, "0.6,100,1.0", "10,20,30")]
        [InlineData(",,", "10,20,30", "1,2,3", ",,", 100, "1,100,3", "10,20,30")]
        [InlineData(",,", "10,20,30", ",,", "1,2,3", 100, "0.6,100,1.0", "10,20,30")]
        [InlineData(",,", "10,20,30", "1,2,3", "4,5,6", 100, "1,100,3", "10,20,30")]
        [InlineData("10,20,30", "40,50,60", ",,", ",,", 100, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", "1,2,3", ",,", 100, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", ",,", "1,2,3", 100, "10,20,30", "40,50,60")]
        [InlineData("10,20,30", "40,50,60", "1,2,3", "4,5,6", 100, "10,20,30", "40,50,60")]
        public void BindTest(string vtn, string vtp, string ax, string bx, double? s, string eVtn, string eVtp) {
            var aBox = ax.Split(',')
                .Select(k => string.IsNullOrEmpty(k) ? (decimal?) null : k.ParseDecimalWithSiPrefix()).ToArray();

            var bBox = bx.Split(',')
                .Select(k => string.IsNullOrEmpty(k) ? (decimal?) null : k.ParseDecimalWithSiPrefix()).ToArray();
            var config = new TransistorPair {
                Vtn = new Transistor {
                    Threshold = aBox[0],
                    Sigma = aBox[1],
                    NumberOfSigma = aBox[2]
                },
                Vtp = new Transistor {
                    Threshold = bBox[0],
                    Sigma = bBox[1],
                    NumberOfSigma = bBox[2]
                }
            };
            var opt = new Opt {
                VtnStrings = vtn.Split(','),
                VtpStrings = vtp.Split(','),
                Sigma = s
            };

            var actual = opt.Bind(config);
            var nBox = eVtn.Split(',').Select(r => r.ParseDecimalWithSiPrefix()).ToArray();
            var pBox = eVtp.Split(',').Select(r => r.ParseDecimalWithSiPrefix()).ToArray();
            var expect = new TransistorPair(nBox[0], nBox[1], nBox[2], pBox[0], pBox[1], pBox[2]);

            Assert.Equal(JsonConvert.SerializeObject(expect), JsonConvert.SerializeObject(actual));
        }

        [Fact]
        public void NullableConfigTest() {
            var opt = new Opt {
                VtnStrings = "1,2,3".Split(','),
                VtpStrings = "4,5,6".Split(','),
                Sigma = null
            };

            var actual = opt.Bind(null);
            Assert.Equal(JsonConvert.SerializeObject(new TransistorPair(1M, 2M, 3M, 4M, 5M, 6M)),
                JsonConvert.SerializeObject(actual)
            );
        }
    }

    public class Opt : ITransistorOption {
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }
    }
}