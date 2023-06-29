using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Services;
using BinarySerializer;
using Shared;
using Common.APIModels;
using Common.Models;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AgentsController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IAgentService _agentService;
        private readonly IFileService _fileService;
        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;
        private readonly IAuditService _auditService;
        private readonly IAgentTaskResultService _agentTaskResultService;

        public AgentsController(IAgentService agentService, IFileService fileService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, IAgentTaskResultService agentTaskResultService)
        {
            this._agentService = agentService;
            this._fileService = fileService;
            this._socksService = socksService;
            this._changeService = changeService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
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

            return Ok(new TeamServerAgent()
            {
                Id = agent.Id,
                FirstSeen = agent.FirstSeen,
                LastSeen = agent.LastSeen,
                RelayId = agent.RelayId,
            });
        }

        [HttpGet("{id}/metadata")]
        public IActionResult GetAgentMetadata(string id)
        {
            var agent = _agentService.GetAgent(id);
            if (agent == null)
                return NotFound();

            return Ok(agent.Metadata);
        }


        //[HttpGet("{agentId}/tasks")]
        //public ActionResult GetTaskresults(string agentId, DateTime? from)
        //{
        //    var agent = this._agentService.GetAgent(agentId);
        //    if (agent is null)
        //        return NotFound("Agent not found");


        //    var results = agent.GetTaskResults();

        //    //if (from.HasValue)
        //    //    results = results.Where(r => r.r)

        //    return Ok(results);
        //}

        [HttpGet("{agentId}/tasks/{taskId}")]
        public ActionResult GetTaskresult(string agentId, string taskId)
        {
            var result = this._agentTaskResultService.GetAgentTaskResult(taskId);
            if (result is null)
                return NotFound("Task not found");

            return Ok(result);
        }

        [HttpPost("{agentId}")]
        public ActionResult TaskAgent(string agentId, [FromBody] CreateTaskRequest ctr)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound();

            byte[] ser = Convert.FromBase64String(ctr.TaskBin);
            var task = ser.BinaryDeserializeAsync<AgentTask>().Result;

            agent.QueueTask(task);
            agent.TaskHistory.Add(new TeamServerAgentTask(ctr.Id, agentId, ctr.Command, DateTime.Now));
            this._changeService.TrackChange(ChangingElement.Task, task.Id);


            //if (!string.IsNullOrEmpty(request.FileId))
            //    agent.QueueDownload(this._fileService.GetFileChunksForAgent(request.FileId));

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";

            this._auditService.Record(this.UserContext, agentId, $"Command tasked to agent : {task.CommandId.ToString()}");

            return Created(path, task);
        }

        //[HttpGet("{agentId}/File")]
        //public ActionResult RequestAgentDowload(string agentId, string fileId)
        //{
        //    var agent = this._agentService.GetAgent(agentId);
        //    if (agent is null)
        //        return NotFound();

        //    agent.QueueDownload(this._fileService.GetFileChunksForAgent(fileId));

        //    return Ok();
        //}

        [HttpGet("{agentId}/stop")]
        public ActionResult StopAgent(string agentId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound("Agent not found");

            this._agentService.RemoveAgent(agent);
            this._changeService.TrackChange(ChangingElement.Agent, agentId);

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
