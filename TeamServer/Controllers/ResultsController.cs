using Microsoft.AspNetCore.Mvc;
using Shared;
using TeamServer.Helper;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly IAgentTaskResultService _resultService;

        public ResultsController(IAgentTaskResultService resultService)
        {
            this._resultService = resultService;
        }


        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            AgentTaskResult res = this._resultService.GetAgentTaskResult(id);
            if (res == null)
                return NotFound(id);

            return Ok(res);
        }

    }
}
