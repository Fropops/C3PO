using ApiModels.Requests;
using ApiModels.Response;
using Commander.Models;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Commander.Communication
{
    public class ApiCommModule : ICommModule
    {
        public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

        public event EventHandler<List<AgentTask>> RunningTaskChanged;
        public event EventHandler AgentsUpdated;

        public event EventHandler<AgentTaskResult> TaskResultUpdated;

        public event EventHandler<string> AgentAdded;

        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;


        protected ConcurrentDictionary<string, Agent> _agents = new ConcurrentDictionary<string, Agent>();
        protected ConcurrentDictionary<string, Listener> _listeners = new ConcurrentDictionary<string, Listener>();
        protected ConcurrentDictionary<string, AgentTask> _tasks = new ConcurrentDictionary<string, AgentTask>();
        protected ConcurrentDictionary<string, AgentTaskResult> _results = new ConcurrentDictionary<string, AgentTaskResult>();

        private ITerminal Terminal;

        public ApiCommModule(ITerminal terminal, string connectAddress, int connectPort)
        {
            this.Terminal = terminal;

            ConnectAddress=connectAddress;
            ConnectPort=connectPort;

            this.UpdateConfig();
        }

        public void UpdateConfig()
        {
            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 5);
            _client.BaseAddress = new Uri($"http://{this.ConnectAddress}:{this.ConnectPort}");
            _client.DefaultRequestHeaders.Clear();

            this._agents.Clear();
            this._tasks.Clear();
            this._results.Clear();
            this._listeners.Clear();

            this.ConnectionStatus = ConnectionStatus.Disconnected;
            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
        }

        public ConnectionStatus ConnectionStatus { get; set; }

        bool firstLoad = true;

        public async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    await this.UpdateAgents();
                    await this.UpdateListeners();
                    await this.UpdateTasks();
                    await this.UpdateResults();

                    if (this.ConnectionStatus != ConnectionStatus.Connected)
                    {
                        this.ConnectionStatus = ConnectionStatus.Connected;
                        this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                    }

                    firstLoad = false;
                }
                catch (Exception e)
                {
                    if ((e.InnerException != null && e.InnerException is TimeoutException) || e is HttpRequestException)
                    {
                        if (this.ConnectionStatus != ConnectionStatus.Disconnected)
                        {
                            this.ConnectionStatus = ConnectionStatus.Disconnected;
                            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                        }
                    }
                    else
                        this.Terminal.WriteError(e.ToString());
                }

                await Task.Delay(5000);
            }
        }


        private async Task UpdateTasks()
        {
            var response = await _client.GetStringAsync("/Tasks/");

            var tasksResponse = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResponse>>(response);

            foreach (var tr in tasksResponse)
            {
                var task = new AgentTask()
                {
                    AgentId = tr.AgentId,
                    Label = tr.Label,
                    Arguments = tr.Arguments,
                    Command = tr.Command,
                    Id = tr.Id,
                    RequestDate = tr.RequestDate,
                };

                this._tasks.AddOrUpdate(tr.Id, task, (key, current) =>
                {
                    current.RequestDate = task.RequestDate;
                    return current;
                });
            }
        }

        private async Task UpdateResults()
        {
            var response = await _client.GetStringAsync("/Tasks/results");
            var resultsResponse = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResultResponse>>(response);

            foreach (var tr in resultsResponse)
            {
                var res = new AgentTaskResult()
                {
                    Id = tr.Id,
                    Result = tr.Result,
                    Info = tr.Info,
                    Status = (AgentResultStatus)tr.Status,
                    FileId = tr.FileId,
                    FileName = tr.FileName,
                };

                //new respone or response change detected

                if (!_results.ContainsKey(res.Id)) // new response
                {
                    if (res.Status == AgentResultStatus.Completed && !firstLoad)
                        this.TaskResultUpdated?.Invoke(this, res);
                }
                else
                {
                    //Change detected :
                    var existing = this._results[res.Id];
                    if (res.Result != existing.Result
                        || res.Status  != existing.Status
                        || res.Info != existing.Info
                        || res.FileId != existing.FileId)
                    {
                        if (res.Status == AgentResultStatus.Completed && !firstLoad)
                            this.TaskResultUpdated?.Invoke(this, res);
                    }
                }

                this._results.AddOrUpdate(tr.Id, res, (key, current) =>
                {
                    current.Result = res.Result;
                    current.Info = res.Info;
                    current.Status = res.Status;
                    return current;
                });

                var running = this._tasks.Values.Where(t => !this._results.ContainsKey(t.Id) || this._results[t.Id].Status != AgentResultStatus.Completed).ToList();
                this.RunningTaskChanged?.Invoke(this, running);
            }
        }





        private async Task UpdateListeners()
        {
            var response = await _client.GetStringAsync("/Listeners/");
            var listenerResponse = JsonConvert.DeserializeObject<IEnumerable<ListenerResponse>>(response);

            this._listeners.Clear();
            foreach (var lr in listenerResponse)
            {
                var listener = new Listener()
                {
                    Name = lr.Name,
                    BindPort = lr.BindPort,
                };

                this._listeners.AddOrUpdate(lr.Name, listener, (key, current) =>
                {
                    return current;
                });
            }
        }

        private async Task UpdateAgents()
        {
            //Terminal.WriteInfo(_client.BaseAddress.ToString());
            var response = await _client.GetStringAsync("Agents");
            var agentResponse = JsonConvert.DeserializeObject<IEnumerable<AgentResponse>>(response);

            var agentIds = agentResponse.Select(a => a.Metadata.Id);


            var addedAgents = new List<string>();

            //del agents
            foreach (var toRemove in this._agents.Keys.Where(k => !agentIds.Contains(k)))
                this._agents.Remove(toRemove, out _);

            //add or update new
            foreach (var ar in agentResponse)
            {
                var agent = new Agent()
                {
                    Metadata = new AgentMetadata()
                    {
                        Architecture = ar.Metadata.Architecture,
                        Hostname = ar.Metadata.Hostname,
                        Id = ar.Metadata.Id,
                        Integrity = ar.Metadata.Integrity,
                        ProcessId = ar.Metadata.ProcessId,
                        ProcessName = ar.Metadata.ProcessName,
                        UserName = ar.Metadata.UserName,
                    },
                    LastSeen = ar.LastSeen
                };

                if (!this._agents.ContainsKey(agent.Metadata.Id))
                    addedAgents.Add(agent.Metadata.Id);

                this._agents.AddOrUpdate(ar.Metadata.Id, agent, (key, current) =>
                {
                    current.LastSeen = agent.LastSeen;
                    return current;
                });
            }

            this.AgentsUpdated?.Invoke(this, new EventArgs());
            foreach (var added in addedAgents)
                this.AgentAdded?.Invoke(this, added);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public List<Agent> GetAgents()
        {
            return this._agents.Values.ToList();
        }

        public Agent GetAgent(int index)
        {
            var agents = GetAgents();
            if (index < 0)
                return null;
            if (index > agents.Count() -1)
                return null;

            return agents[index];
        }

        public Agent GetAgent(string id)
        {
            return this._agents[id];
        }

        public void DeleteAgent(string id)
        {
            if (GetAgent(id) != null)
                this._agents.Remove(id, out var ret);
        }

        public IEnumerable<AgentTask> GetTasks(string id)
        {
            return this._tasks.Values.Where(t => t.AgentId == id).OrderByDescending(t => t.RequestDate);
        }

        public async Task<HttpResponseMessage> StopAgent(string id)
        {
            this.DeleteAgent(id);
            return await _client.GetAsync($"/Agents/{id}/stop");
        }

        public AgentTask GetTask(string taskId)
        {
            if (!this._tasks.ContainsKey(taskId))
                return null;
            return this._tasks[taskId];
        }

        public AgentTaskResult GetTaskResult(string taskId)
        {
            if (!this._results.ContainsKey(taskId))
                return null;
            return this._results[taskId];
        }



        public async Task<HttpResponseMessage> CreateListener(string name, int port)
        {
            var requestObj = new ApiModels.Requests.StartHttpListenerRequest();
            requestObj.Name = name;
            requestObj.BindPort  = port;
            var requestContent = JsonConvert.SerializeObject(requestObj);

            return await _client.PostAsync("/Listeners/", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
        }

        public IEnumerable<Listener> GetListeners()
        {
            return this._listeners.Values.ToList();
        }



        public async Task TaskAgent(string label, string taskId, string agentId, string cmd, string parms = null)
        {
            var taskrequest = new TaskAgentRequest()
            {
                Label = label,
                Id = taskId,
                Command = cmd,
            };
            if (!string.IsNullOrEmpty(parms))
                taskrequest.Arguments = parms;

            var requestContent = JsonConvert.SerializeObject(taskrequest);

            var response = await _client.PostAsync($"/Agents/{agentId}", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task TaskAgent(string label, string taskId, string agentId, string cmd, string fileId, string fileName, string parms = null)
        {
            var taskrequest = new TaskAgentRequest()
            {
                Label = label,
                Id = taskId,
                Command = cmd,
                FileId = fileId,
                FileName = fileName,
            };
            if (!string.IsNullOrEmpty(parms))
                taskrequest.Arguments = parms;

            var requestContent = JsonConvert.SerializeObject(taskrequest);

            var response = await _client.PostAsync($"/Agents/{agentId}", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }





        private async Task<FileDescriptorResponse> SetupDownload(string id)
        {
            var response = await _client.GetAsync($"/Files/SetupDownload/{id}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var desc = JsonConvert.DeserializeObject<FileDescriptorResponse>(json);
            return desc;
        }

        private async Task<FileChunckResponse> GetFileChunk(string id, int chunckIndex)
        {
            var response = await _client.GetAsync($"/Files/Download/{id}/{chunckIndex}");


            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var chunck = JsonConvert.DeserializeObject<FileChunckResponse>(json);
            return chunck;
        }

        private async Task SetupUpload(FileDescriptorResponse fileDesc)
        {
            var requestContent = JsonConvert.SerializeObject(fileDesc);

            var response = await _client.PostAsync($"/Files/SetupUpload", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        private async Task PostFileChunk(FileChunckResponse chunk)
        {
            var requestContent = JsonConvert.SerializeObject(chunk);
            var response = await _client.PostAsync($"/Files/Upload", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json")); ;
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task<Byte[]> Download(string id, Action<int> OnCompletionChanged = null)
        {
            var desc = await this.SetupDownload(id);
            var chunks = new List<FileChunckResponse>();

            for (int index = 0; index < desc.ChunkCount; ++index)
            {
                var chunk = this.GetFileChunk(desc.Id, index).Result;
                chunks.Add(chunk);
                OnCompletionChanged?.Invoke(index * 100 / desc.ChunkCount);
            }

            OnCompletionChanged?.Invoke(100);
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

        public async Task<string> Upload(byte[] fileBytes, string filename, Action<int> OnCompletionChanged = null)
        {

            var desc = new FileDescriptorResponse()
            {
                Length = fileBytes.Length,
                ChunkSize = ChunkSize,
                Id = Guid.NewGuid().ToString(),
                Name = filename
            };

            var chunks = new List<FileChunckResponse>();

            int index = 0;
            using (var ms = new MemoryStream(fileBytes))
            {

                var buffer = new byte[ChunkSize];
                int numBytesToRead = (int)ms.Length;

                while (numBytesToRead > 0)
                {

                    int n = ms.Read(buffer, 0, ChunkSize);
                    //var data =
                    var chunk = new FileChunckResponse()
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
            OnCompletionChanged?.Invoke(100);

            return desc.Id;
        }
    }
}
