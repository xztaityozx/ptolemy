using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ptolemy.Argo.Request;

namespace Ptolemy.Hydra.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class GetController : ControllerBase {
        [HttpGet]
        public IEnumerable<ArgoRequest> GetPendingTasks() {
            var dir = Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "task");
            return !Directory.Exists(dir) ? null : Directory.GetFiles(dir).Select(ArgoRequest.FromFile);
        }

    }
}