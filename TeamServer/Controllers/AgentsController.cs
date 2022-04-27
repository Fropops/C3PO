using ApiModels.Requests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentsController(IAgentService agentService)
        {
            this._agentService = agentService;
        }

        [HttpGet]
        public IActionResult GetAgents()
        {
            var agents = _agentService.GetAgents();
            return Ok(agents);
        }

        [HttpGet("{name}")]
        public IActionResult GetAgent(string name)
        {
            var agent = _agentService.GetAgent(name);
            if (agent == null)
                return NotFound();

            return Ok(agent);
        }


        [HttpGet("{agentId}/tasks")]
        public ActionResult GetTaskresults(string agentId, DateTime? from)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound("Agent not found");

            
            var results = agent.GetTaskResults();

            //if (from.HasValue)
            //    results = results.Where(r => r.r)

            return Ok(results);
        }

        [HttpGet("{agentId}/tasks/{taskId}")]
        public ActionResult GetTaskresult(string agentId, string taskId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound("Agent not found");

            var result = agent.GetTaskResult(taskId);
            if (result is null)
                return NotFound("Task not found");

            return Ok(result);
        }

        [HttpPost("{agentId}")]
        public ActionResult TaskAgent(string agentId, [FromBody] TaskAgentRequest request)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound();

            var task = new AgentTask()
            {
                Id = Guid.NewGuid().ToString(),
                Command = request.Command,
                Arguments = request.Arguments,
                File = request.File,
                RequestDate = DateTime.UtcNow
            };

            agent.QueueTask(task);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";
            return Created(path, task);
        }

        [HttpGet("{agentId}/stop")]
        public ActionResult StopAgent(string agentId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound("Agent not found");

            this._agentService.RemoveAgent(agent);

            return Ok();
        }

    }
}
