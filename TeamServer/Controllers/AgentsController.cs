using ApiModels.Requests;
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
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IFileService _fileService;
        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;

        public AgentsController(IAgentService agentService, IFileService fileService, ISocksService socksService, IChangeTrackingService changeService)
        {
            this._agentService = agentService;
            this._fileService = fileService;
            this._socksService = socksService;
            this._changeService = changeService;
        }

        [HttpGet]
        public IActionResult GetAgents()
        {
            var agents = _agentService.GetAgents();
            return Ok(agents);
        }

        [HttpGet("{id}")]
        public IActionResult GetAgent(string id)
        {
            var agent = _agentService.GetAgent(id);
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
                Id = request.Id,
                Command = request.Command,
                Arguments = request.Arguments,
                Label = request.Label,
                RequestDate = DateTime.UtcNow,
                FileName = request.FileName,
                FileId = request.FileId,
            };

            agent.QueueTask(task);
            this._changeService.TrackChange(ApiModels.Changes.ChangingElement.Task, request.Id);
            if (!string.IsNullOrEmpty(request.FileId))
                agent.QueueDownload(this._fileService.GetFileChunksForAgent(request.FileId));

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
            this._changeService.TrackChange(ApiModels.Changes.ChangingElement.Agent, agentId);

            return Ok();
        }

        [HttpGet("{agentId}/startproxy")]
        public ActionResult StartProxy(string agentId, int port)
        {
            if (!this._socksService.StartProxy(agentId, port))
                return this.Problem($"Cannot start proxy on port {port}!");

            return Ok();
        }

        [HttpGet("{agentId}/stopproxy")]
        public ActionResult StopProxy(string agentId)
        {
            if (!this._socksService.StopProxy(agentId))
                return this.Problem($"Cannot stop proxy!");

            return Ok();
        }

    }
}
