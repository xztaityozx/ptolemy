using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ptolemy.Argo.Request;
using Ptolemy.Aries;
using Ptolemy.Config;
using Ptolemy.Hydra.Controllers;
using Ptolemy.Hydra.Request;
using Ptolemy.Parameters;
using Xunit;

namespace UnitTest.HydraTest {
    public class MakeControllerTest {
        private static IEnumerable<ArgoRequest> GenerateRequests() {
            return new[] {
                new ArgoRequest(),
                new ArgoRequest {
                    HspicePath = "path", NetList = "netlist",
                    Transistors = new TransistorPair(null, new Transistor(1, 2, 3M)),
                    Vdd = 10, Gnd = 2,
                },
                new ArgoRequest {
                    HspicePath = "path", NetList = "netlist",
                    Transistors = new TransistorPair(new Transistor(1, 2, 3M), null),
                    Vdd = 10, Gnd = 2,
                },
                new ArgoRequest {
                    HspicePath = "path", NetList = "netlist",
                    Transistors = new TransistorPair((1.0, 2.0, 3.0), (4.0, 5.0, 6.0)),
                    Gnd = 10, Vdd = 10
                },
                new ArgoRequest {
                    Transistors = new TransistorPair((1.0, 2.0, 3.0), (4.0, 5.0, 6.0)),
                    Vdd = 10, Gnd = 2,
                },
                ArgoRequest.FromJson(
                    "{\"Seed\":2, \"Sweep\":200, \"SweepStart\":1, \"Temperature\": 25.0, \"Transistors\":{ \"Vtn\":{ \"Threshold\":0.6,\"Sigma\":0.046, \"Deviation\":1.0}, \"Vtp\":{ \"Threshold\":-0.6, \"Sigma\": 0.046, \"Deviation\":1.0} },\"Time\":{ \"Start\":0, \"Step\":0.00001, \"Stop\":0.04}, \"IcCommands\": [\"V(N1)\", \"V(N2\"], \"NetList\":\"Netlist\",\"Includes\":[\"modelFile\"], \"Vdd\":0.8, \"Gnd\":0, \"Signals\":[\"N1\", \"N2\"], \"HspicePath\": \"/path/to/hspice\",\"PlotTimeList\":[0.04]}")
            };
        }

        [Fact]
        public void MakeRequests_HydraRequest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Hydra", "Test");
            Directory.CreateDirectory(tmp);
            Config.Assign(new Config {
                WorkingRoot = tmp, ArgoDefault = new ArgoRequest {Transistors = new TransistorPair(0, 0, 0, 0, 0, 0.0)}
            });
            var dir = Path.Combine(Config.Instance.WorkingRoot, "aries", "task");
            Directory.CreateDirectory(dir);

            try {
                var netlist = Path.Combine(tmp, "netlist");
                {
                    using var sw = new StreamWriter(netlist);
                    sw.WriteLine("netlist");
                }
                var req = new HydraRequest {
                    AriesMake = new AriesMake {
                        Seed = "1", SweepStart = "1", NetList = netlist,
                        HspicePath = netlist, TotalSweeps = "100", Signals = new List<string> {"a", "b"},
                        Gnd = "0", Vdd = "10", IcCommands = new List<string> {"VV", "XX"},
                        Includes = new List<string> {"include"}, PlotTimeRequest = "4,5,6",
                        SplitOption = "none", Temperature = "25", TimeString = "0,100p,4n",
                        VtnStrings = new List<string> {"10", "20", "30"},
                        VtpStrings = new List<string> {"40", "50", "60"},
                        Options = new List<string>()
                    }
                };

                var c = new MakeController();
                var res = c.Make(req);

                var status = Assert.IsType<OkResult>(res);
                Assert.Equal(200, status.StatusCode);

                Assert.True(Directory.Exists(dir));
                var files = Directory.GetFiles(dir);
                Assert.Single(files);
            }
            finally {
                Directory.Delete(tmp, true);
                Config.Assign(null);
            }
        }

        [Fact]
        public void MakeRequests_BadRequest() {
            var req = GenerateRequests().Take(5).ToArray();
            var c = new MakeController();
            var res = c.Make(req);

            var status = Assert.IsType<BadRequestObjectResult>(res);
            Assert.Equal(
                JsonConvert.SerializeObject(new {message = "requests are invalid"}),
                JsonConvert.SerializeObject(status.Value));
        }

        [Fact]
        public void MakeRequests_Created() {
            Config.Assign(new Config {WorkingRoot = Path.Combine(Path.GetTempPath(), "Ptolemy.Hydra", "Test"), ArgoDefault = new ArgoRequest {
                HspicePath = "/path/to/hspice", HspiceOptions = new List<string>()
            }});
            var dir = Path.Combine(Config.Instance.WorkingRoot, "aries", "task");
            try {
                var req = GenerateRequests().ToArray();

                Directory.CreateDirectory(Config.Instance.WorkingRoot);
                var c = new MakeController();
                var res = c.Make(req);

                var status = Assert.IsType<CreatedAtActionResult>(res);
                Assert.Null(status.ControllerName);
                Assert.Equal("Make", status.ActionName);
                Assert.Equal(
                    JsonConvert.SerializeObject(new {accepted = new[] {5}, rejected = new[] {0, 1, 2, 3, 4}}),
                    JsonConvert.SerializeObject(status.Value)
                );

                Assert.True(Directory.Exists(dir));
                var items = Directory.GetFiles(dir);
                Assert.Single(items);

                Assert.Equal(
                    ArgoRequest.FromFile(items[0]).ToJson(),
                    req.Last().ToJson()
                );
            }
            finally {
                Directory.Delete(Config.Instance.WorkingRoot, true);
                Config.Assign(null);
            }
        }
    }
}
