using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Parameters;
using Ptolemy.Verb;
using Xunit;

namespace UnitTest {
    public class VerbTest {
        private static TestVerb Start(IEnumerable<string> param) {
            return Parser.Default.ParseArguments<TestVerb>(param).MapResult(
                t => {
                    t.Run(CancellationToken.None);
                    return t;
                },
                e => null
            );
        }

        [Fact]
        public void BindTest_SetToDefault() {
            var param = "test".Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var res = Start(param);

            Assert.Equal($"{new Transistor(0.6, 0.046, 1.0)}", $"{res.N}");
            Assert.Equal($"{new Transistor(-0.6, 0.046, 1.0)}", $"{res.P}");
        }

        [Fact]
        public void BindTest_FromString() {
            var data = new[] {
                new {
                    param = "--vtn 1,2,3 --vtp 4,5,6",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(4.0, 5.0, 6.0)
                },
                new {
                    param = "--vtn 1.0,2.0,3.0 --vtp 4.0,5.0,6.0",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(4.0, 5.0, 6.0)
                },
                new {
                    param = "--vtn 1,2,3",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param = "--vtp 4.0,5.0,6.0",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(4.0, 5.0, 6.0)
                },
                new {
                    param = "--vtn 1K,2M,3G",
                    vtn = new Transistor(1.0E3, 2.0E6, 3.0E9),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param = "--vtn ,, --sigma 8",
                    vtn = new Transistor(0.6, 8, 1.0),
                    vtp = new Transistor(-0.6, 8, 1.0)
                },
                new {
                    param = "--vtn 1,,3 --vtp 4,,6 --sigma 8",
                    vtn = new Transistor(1.0, 8, 3.0),
                    vtp = new Transistor(4.0, 8, 6.0)
                },
                new {
                    param = "--vtn ,,3 --vtp ,,6",
                    vtn = new Transistor(0.6, 0.046, 3.0),
                    vtp = new Transistor(-0.6, 0.046, 6.0)
                },
                new {
                    param = "--vtn 1,2, --vtp 3,4,",
                    vtn = new Transistor(1.0, 2.0, 1.0),
                    vtp = new Transistor(3.0, 4.0, 1.0)
                },
                new {
                    param = "--vtn ,2, --vtp ,4,",
                    vtn = new Transistor(0.6, 2.0, 1.0),
                    vtp = new Transistor(-0.6, 4.0, 1.0)
                }
            };
            foreach (var d in data) {
                var res = Start(d.param.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                Assert.Equal($"{d.vtn}", $"{res.N}");
                Assert.Equal($"{d.vtp}", $"{res.P}");
            }
        }

        [Fact]
        public void BindTest_FromOptions() {
            var data = new[] {
                new {
                    param = "--vtnThreshold 1.0",
                    vtn = new Transistor(1.0, 0.046, 1.0),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param = "--vtnSigma 1.0",
                    vtn = new Transistor(0.6, 1, 1.0),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param = "--vtnDeviation 8",
                    vtn = new Transistor(0.6, 0.046, 8),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param = "--vtpThreshold 1.0",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(1.0, 0.046, 1.0)
                },
                new {
                    param = "--vtpSigma 1.0",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(-0.6, 1, 1.0)
                },
                new {
                    param = "--vtpDeviation 8",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(-0.6, 0.046, 8)
                },

                new {
                    param="--vtnThreshold 1.0 --vtnSigma 2.0 --vtnDeviation 3.0 --vtpThreshold 4.0 --vtpSigma 5.0 --vtpDeviation 6.0",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(4.0, 5.0, 6.0)
                },
                new {
                    param="--vtn 1,2,3 --vtnThreshold 7 --vtnSigma 8 --vtnDeviation 9 --vtpThreshold 4.0 --vtpSigma 5.0 --vtpDeviation 6.0",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(4.0, 5.0, 6.0)
                },
                new {
                    param="--vtp 7,8,9 --vtnThreshold 1 --vtnSigma 2 --vtnDeviation 3 --vtpThreshold 4.0 --vtpSigma 5.0 --vtpDeviation 6.0",
                    vtn = new Transistor(1.0, 2.0, 3.0),
                    vtp = new Transistor(7.0, 8.0, 9.0)
                },
                new {
                    param="--vtn 1,2, --vtnDeviation 9",
                    vtn = new Transistor(1.0, 2.0, 9.0),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param="--vtn 1,,3 --vtnSigma 9",
                    vtn = new Transistor(1.0, 9, 3.0),
                    vtp = new Transistor(-0.6, 0.046, 1.0)
                },
                new {
                    param="--vtn ,2,3 --vtnThreshold 9",
                    vtn = new Transistor(9.0, 2.0, 3.0),
                    vtp = new Transistor(-0.6,0.046, 1.0)
                },
                new {
                    param="--vtp 1,2, --vtpDeviation 9",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(1, 2, 9.0)
                },
                new {
                    param="--vtp 1,,3 --vtpSigma 9",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(1.0, 9.0, 3.0)
                },
                new {
                    param="--vtp ,2,3 --vtpThreshold 9",
                    vtn = new Transistor(0.6, 0.046, 1.0),
                    vtp = new Transistor(9.0, 2.0, 3.0)
                },
            };
            foreach (var d in data) {
                var res = Start(d.param.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                Assert.Equal($"{d.vtn}", $"{res.N}");
                Assert.Equal($"{d.vtp}", $"{res.P}");
            }
        }

    }

    [Verb("test")]
    internal class TestVerb : Verb {
        protected override void Do(CancellationToken token) {

        }

        public Transistor N => Vtn;
        public Transistor P => Vtp;
    }
}
