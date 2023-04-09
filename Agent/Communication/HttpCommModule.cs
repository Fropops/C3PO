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
            _client.DefaultRequestHeaders.Add("Authorization", messManager.AgentMetaData.Id);
            
            //_client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ GenerateToken());

        }

        //private string GenerateToken()
        //{
        //    var id = this.MessageService.AgentMetaData.Id;
        //    var secretKey = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

        //    RijndaelManaged rijndael = new RijndaelManaged();
        //    rijndael.KeySize = 256;
        //    rijndael.BlockSize = 128;
        //    rijndael.Key = decryptedKey;
        //    rijndael.IV = decryptedIv;
        //    rijndael.Padding = PaddingMode.PKCS7;
        //}


        protected override async Task<List<MessageTask>> CheckIn(List<MessageResult> results)
        {
            var content = new StringContent(Encoding.UTF8.GetString(results.Serialize()), Encoding.UTF8, "application/json");
            var file = ShortGuid.NewGuid() + ".jpeg";
            var response = await _client.PostAsync($"/", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            return responseContent.Deserialize<List<MessageTask>>();
        }


    }
}
