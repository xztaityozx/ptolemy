using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Ptolemy.Argo.Request;
using Ptolemy.Hydra.Request;

namespace Ptolemy.Hydra.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class MakeController : ControllerBase {

        [HttpPost]
        [Route("hydra")]
        public IActionResult Make(HydraRequest request) {
            request.AriesMake.Run(CancellationToken.None);
            return Ok();
        }

        [HttpPost]
        [Route("argo")]
        public IActionResult Make(ArgoRequest[] requests) {
            var dir = Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "task");
            Directory.CreateDirectory(dir);

            // list of accepted item's index
            var accepted = new List<int>();
            // list of rejected item's index
            var rejected = new List<int>();


            var groupId = Guid.NewGuid();

            foreach (var item in requests.Select((request, i) => new {request, i})) {
                if (item.request.IsSimulationable()) {
                    item.request.GroupId = groupId;
                    
                    // Auto bind hspice setting
                    item.request.HspicePath = Config.Config.Instance.ArgoDefault.HspicePath;
                    item.request.HspiceOptions = Config.Config.Instance.ArgoDefault.HspiceOptions;

                    // filename: {dir}/{groupId}-{index}.json
                    var path = Path.Combine(dir, $"{groupId}-{item.i}.json");
                    using (var sw = new StreamWriter(path)) {
                        sw.WriteLine(item.request.ToJson());
                        sw.Flush();
                    }

                    accepted.Add(item.i);
                }
                else rejected.Add(item.i);
            }

            if (accepted.Any()) return CreatedAtAction("Make", new {accepted, rejected});
            return BadRequest(new {message = "requests are invalid"});
        }
    }
}