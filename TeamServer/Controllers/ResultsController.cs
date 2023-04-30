using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public ResultsController(IAgentService agentService)
        {
            this._agentService = agentService;
        }


        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            AgentTaskResultResponse result = null;
            foreach(var agent in this._agentService.GetAgents())
            {
                var r = agent.GetTaskResult(id);
                if (r != null)
                {
                    result = new AgentTaskResultResponse()
                    {
                        Id = r.Id,
                        Info = r.Info,
                        Result = r.Result,
                        Error = r.Error,
                        Objects = r.Objects,
                        Status = (int)r.Status
                    };
                    foreach(var file in r.Files)
                    {
                        result.Files.Add(new ApiModels.Response.TaskFileResult()
                        {
                            FileId = file.FileId,
                            FileName = file.FileName,
                            IsDownloaded = file.IsDownloaded,
                        });
                    }
                    break;
                }
            }

            if (result != null)
                return Ok(result);
            else
                return NotFound(result);
        }

    }
}
