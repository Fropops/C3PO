using Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TeamServer.Helper;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public TasksController(IAgentService agentService)
        {
            this._agentService = agentService;
        }

        //[HttpGet]
        //public IActionResult GetTasks()
        //{
        //    var results = new List<AgentTaskResponse>();
        //    var agents = _agentService.GetAgents();

        //   foreach(var agent in agents)
        //    {
        //        foreach(var task in agent.TaskHistory)
        //        {
        //            results.Add(new AgentTaskResponse()
        //            {
        //                Label = task.Label,
        //                AgentId = agent.Metadata.Id,
        //                Arguments = task.Arguments,
        //                Command = task.Command,
        //                Id = task.Id,
        //                RequestDate = task.RequestDate,
        //            });
        //        }
                
        //    }
        //    return Ok(results);
        //}

        [HttpGet("{id}")]
        public ActionResult Result(string id)
        {
            TeamServerAgentTask task = null;
            foreach (var agent in this._agentService.GetAgents())
            {
                task = agent.TaskHistory.FirstOrDefault(t => t.Id == id);
                if (task != null)
                    break;
            }

            if (task != null)
                return Ok(task);
            else
                return NotFound();
        }


        //[HttpGet("results")]
        //public IActionResult GetResults()
        //{
        //    var results = new List<AgentTaskResultResponse>();
        //    var agents = _agentService.GetAgents();

        //    foreach (var agent in agents)
        //    {
        //        foreach (var res in agent.GetTaskResults())
        //        {
        //            var ret = new AgentTaskResultResponse()
        //            {
        //                Id = res.Id,
        //                Result = res.Result,
        //                Info = res.Info,
        //                Status = (int)res.Status,
        //            };

        //            foreach(var file in res.Files)
        //            ret.Files.Add(new ApiModels.Response.TaskFileResult()
        //            {
        //                FileId = file.FileId,
        //                FileName = file.FileName,
        //                IsDownloaded = file.IsDownloaded,
        //            });

        //            results.Add(ret);
        //        }

        //    }
        //    return Ok(results);
        //}
    }
}
