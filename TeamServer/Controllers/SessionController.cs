using ApiModels.Changes;
using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class SessionController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IAuditService _auditService;

        public SessionController(IChangeTrackingService trackService, IListenerService listenerService, IAgentService agentService, IAuditService auditService)
        {
            _changeTrackingService = trackService;
            _listenerService = listenerService;
            _agentService = agentService;
            _auditService = auditService;
        }

        [HttpGet("changes")]
        public IActionResult Changes(bool history = false)
        {
            var session = this.UserContext.Session;
            if (!this._changeTrackingService.ContainsSession(session))
            {
                this._auditService.Record(this.UserContext, $"Connexion to TeamServer");
            }

            if (!history)
                return Ok(this._changeTrackingService.ConsumeChanges(this.UserContext.Session).OrderBy(c => c.Element));
            else
            {
                List<Change> changes = new List<Change>();
                foreach (var listener in this._listenerService.GetListeners())
                    changes.Add(new Change(ChangingElement.Listener, listener.Id));
                foreach (var agent in this._agentService.GetAgents())
                {
                    changes.Add(new Change(ChangingElement.Agent, agent.Id));
                    foreach (var task in agent.TaskHistory)
                        changes.Add(new Change(ChangingElement.Task, task.Id));
                    foreach (var res in agent.GetTaskResults())
                        changes.Add(new Change(ChangingElement.Result, res.Id));

                }

                return Ok(changes);
            }
        }

        [HttpGet("exit")]
        public IActionResult Exit()
        {
            var session = this.UserContext.Session;
            if (this._changeTrackingService.ContainsSession(session))
            {
                this._auditService.Record(this.UserContext, $"Exit TeamServer");
                return this.Problem();
            }
            this._changeTrackingService.CleanSession(session);
            return Ok();
        }

    }
}
