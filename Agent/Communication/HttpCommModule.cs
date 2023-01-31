using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{
    public class HttpCommModule : CommModule
    {
        private HttpClient _client;
        public HttpCommModule(ConnexionUrl conn, IMessageService messManager, IFileService fileService, IProxyService proxyService) : base(conn, messManager, fileService, proxyService)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback
            (
               delegate
               {
                   return true;
               }
            );

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            //_client.BaseAddress = new Uri($"https://{this.ConnectAddress}:{this.ConnectPort
            _client.BaseAddress = new Uri($"{conn}");
            //Console.WriteLine(_client.BaseAddress);
            _client.DefaultRequestHeaders.Clear();

        }


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var content = new StringContent(Encoding.UTF8.GetString(results.Serialize()), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/ci/{this.MessageService.AgentMetaData.Id}", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            return responseContent.Deserialize<List<MessageTask>>();
        }

      
    }
}
