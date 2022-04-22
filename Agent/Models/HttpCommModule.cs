using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class HttpCommModule : CommModule
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;

        public HttpCommModule(string connectAddress, int connectPort)
        {
            ConnectAddress=connectAddress;
            ConnectPort=connectPort;
        }

        public override void Init(AgentMetadata metadata)
        {
            base.Init(metadata);

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            _client.BaseAddress = new Uri($"http://{this.ConnectAddress}:{this.ConnectPort}");
            _client.DefaultRequestHeaders.Clear();

            var encodedMetadata = Convert.ToBase64String(metadata.Serialize());
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");

        }


        public override async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!_outBound.IsEmpty)
                    {
                        await PostData();
                    }
                    else
                        await this.CheckIn();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }

                await Task.Delay(2000);
            }
        }

        private async Task CheckIn()
        {
            var response = await _client.GetByteArrayAsync("/");
            HandleResponse(response);
        }

        private async Task PostData()
        {
            var outbound = GetOutbound().ToArray().Serialize();
            var content = new StringContent(Encoding.UTF8.GetString(outbound), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            this.HandleResponse(responseContent);
        }

        private void HandleResponse(byte[] response)
        {
            var tasks = response.Deserialize<AgentTask[]>();
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                    this._inbound.Enqueue(task);
            }
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }

        private async Task<FileDescriptor> SetupDownload(int filetype, string filename)
        {
            var response = await _client.GetByteArrayAsync($"/SetupDownload?filetype={filetype}&filename={filename}");
            //var json = Encoding.UTF8.GetString(response);
            return response.Deserialize<FileDescriptor>();
        }

        private async Task<FileChunk> GetFileChunk(string id, int chunckIndex)
        {
            var response = await _client.GetByteArrayAsync($"/DownloadChunk?id={id}&index={chunckIndex}");
            return response.Deserialize<FileChunk>();
        }


        public override async Task<Byte[]> Download(int filetype, string filename, Action<int> OnCompletionChanged = null)
        {
            var desc = await this.SetupDownload(filetype, filename);
            var chunks = new List<FileChunk>();

            for (int index = 0; index < desc.ChunkCount; ++index)
            {
                var chunk = this.GetFileChunk(desc.Id, index).Result;
                chunks.Add(chunk);
                OnCompletionChanged?.Invoke(index * 100 / desc.ChunkCount);
            }

            using (var ms = new MemoryStream())
            {
                foreach (var chunk in chunks.OrderBy(c => c.Index))
                {
                    var bytes = Convert.FromBase64String(chunk.Data);
                    ms.Write(bytes, 0, bytes.Length);
                }

                return ms.ToArray();

            }
        }
    }
}
