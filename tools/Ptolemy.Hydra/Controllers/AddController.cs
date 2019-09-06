using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ptolemy.Hydra.Request;

namespace Ptolemy.Hydra.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AddController : ControllerBase {
        [HttpPost]
        public ActionResult<string> Add(HydraRequest request) {
            return request.SweepSplitOption.ToString();
        }
    }
}