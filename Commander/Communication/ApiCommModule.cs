using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiModels.Requests;
using ApiModels.Response;
using ApiModels.Changes;
using Commander.Models;
using Commander.Terminal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Spectre.Console;
using ApiModels;
using ApiModels.WebHost;

namespace Commander.Communication
{
    public class ApiCommModule : ICommModule
    {
        public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

        public event EventHandler<List<AgentTask>> RunningTaskChanged;
        public event EventHandler AgentsUpdated;

        public event EventHandler<AgentTaskResult> TaskResultUpdated;

        public event EventHandler<Agent> AgentAdded;

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;


        protected ConcurrentDictionary<string, Agent> _agents = new ConcurrentDictionary<string, Agent>();
        protected ConcurrentDictionary<string, Listener> _listeners = new ConcurrentDictionary<string, Listener>();
        protected ConcurrentDictionary<string, AgentTask> _tasks = new ConcurrentDictionary<string, AgentTask>();
        protected ConcurrentDictionary<string, AgentTaskResult> _results = new ConcurrentDictionary<string, AgentTaskResult>();

        private ITerminal Terminal;

        public CommanderConfig Config { get; set; }
        public ApiCommModule(ITerminal terminal, CommanderConfig config)
        {
            this.Terminal = terminal;
            this.Config = config;

            this.UpdateConfig();
        }

        public void UpdateConfig()
        {
            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 5);
            _client.BaseAddress = new Uri($"http://{this.Config.ApiConfig.EndPoint}");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ GenerateToken());

            this._agents.Clear();
            this._tasks.Clear();
            this._results.Clear();
            this._listeners.Clear();

            this.ConnectionStatus = ConnectionStatus.Unknown;
            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
        }

        private string GenerateToken()
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.Config.ApiConfig.ApiKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", Config.ApiConfig.User), new Claim("session", Config.Session) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Unknown;

        bool isSyncing = true;
        public async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var changes = await this.GetChanges(this.isSyncing);

                    if (this.ConnectionStatus != ConnectionStatus.Connected)
                    {
                        this.ConnectionStatus = ConnectionStatus.Connected;
                        this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                    }

                    if (isSyncing)
                    {
                        var serverConfig = await this.ServerConfig();
                        this.Config.ServerConfig = serverConfig;
                        

                        this._listeners.Clear();
                        this._agents.Clear();
                        this._tasks.Clear();
                        this._results.Clear();

                        Terminal.Interrupt();
                        Terminal.CanHandleInput = false;

                        await AnsiConsole.Progress()
                            .Columns(new ProgressColumn[]
                            {
                                new TaskDescriptionColumn(),    // Task description
                                new ProgressBarColumn(),        // Progress bar
                                new PercentageColumn(),         // Percentage
                                new SpinnerColumn(Spinner.Known.Default).Style(Style.Parse("cyan")),            // Spinner
                            })
                        .StartAsync(async ctx =>
                        {
                            var task1 = ctx.AddTask($"[cyan]Syncing whith TeamServer ({changes.Count} items)[/]");

                            task1.MaxValue = changes.Count;
                            //// Simulate some work
                            //AnsiConsole.MarkupLine("Doing some more work...");
                            //Thread.Sleep(2000);
                            foreach (var change in changes)
                            {
                                //await Task.Delay(10000);
                                await this.HandleChange(change);
                                task1.Increment(1);
                            }
                        });


                        //Terminal.WriteInfo($"Syncing whith TeamServer ({changes.Count}) :");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Listener)} Listeners to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Agent)} Agents to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Task)} Tasks to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Result)} Results to load.");

                        Terminal.WriteSuccess($"Syncing done.");
                        //Terminal.WriteInfo($"ServerKey is {this.Config.ServerConfig.Key}");
                        Terminal.Restore();
                        Terminal.CanHandleInput = true;
                    }
                    else
                    {
                        foreach (var change in changes)
                            await this.HandleChange(change);
                    }

                    isSyncing = false;
                }
                catch (Exception e)
                {
                    if ((e.InnerException != null && e.InnerException is TimeoutException) || e is HttpRequestException)
                    {
                        var newStatus = ConnectionStatus.Disconnected;
                        if (e is HttpRequestException)
                        {
                            var htEx = e as HttpRequestException;
                            if (htEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                newStatus = ConnectionStatus.Unauthorized;
                        }


                        if (this.ConnectionStatus != newStatus)
                        {
                            this.ConnectionStatus = newStatus;
                            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                        }
                    }
                    else
                        this.Terminal.WriteError(e.ToString());
                }

                await Task.Delay(this.Config.ApiConfig.Delay);
            }
        }

        private async Task HandleChange(Change change)
        {
            switch (change.Element)
            {
                case ChangingElement.Listener:
                    await this.UpdateListener(change.Id);
                    break;
                case ChangingElement.Agent:
                    await this.UpdateAgent(change.Id);
                    break;
                case ChangingElement.Task:
                    await this.UpdateTask(change.Id);
                    break;
                case ChangingElement.Result:
                    await this.UpdateResult(change.Id);
                    break;
            }
        }

        private async Task<List<Change>> GetChanges(bool first)
        {
            var response = await _client.GetStringAsync($"/Changes?history={first}");
            var tasksResponse = JsonConvert.DeserializeObject<List<Change>>(response);
            return tasksResponse;
        }

        private async Task UpdateListener(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"/Listeners/{id}");
                var lr = JsonConvert.DeserializeObject<ListenerResponse>(response);

                var listener = new Listener()
                {
                    Name = lr.Name,
                    Id = lr.Id,
                    BindPort = lr.BindPort,
                    Secured = lr.Secured,

                    Ip = lr.Ip,
                };

                this._listeners.AddOrUpdate(lr.Id, listener, (key, current) =>
                {
                    current.Name = listener.Name;
                    current.BindPort = listener.BindPort;
                    current.Secured = listener.Secured;
                    current.Ip = listener.Ip;
                    return current;
                });
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._listeners.ContainsKey(id))
                        this._listeners.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateAgent(string id)
        {
            try
            {
                //Terminal.WriteInfo(_client.BaseAddress.ToString());
                var response = await _client.GetStringAsync($"Agents/{id}");
                var ar = JsonConvert.DeserializeObject<AgentResponse>(response);

                //add or update new
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
                        EndPoint = ar.Metadata.EndPoint,
                        Version = ar.Metadata.Version,
                        SleepInterval = ar.Metadata.SleepInterval,
                        SleepJitter = ar.Metadata.SleepJitter,
                    },
                    LastSeen = ar.LastSeen,
                    FirstSeen = ar.FirstSeen,
                    ListenerId = ar.ListenerId,
                    Path = ar.Path
                };

                bool isNew = !this._agents.ContainsKey(agent.Metadata.Id);
                this._agents.AddOrUpdate(ar.Metadata.Id, agent, (key, current) =>
                {
                    current.Metadata.Architecture = agent.Metadata.Architecture;
                    current.Metadata.Hostname = agent.Metadata.Hostname;
                    current.Metadata.Integrity = agent.Metadata.Integrity;
                    current.Metadata.ProcessId = agent.Metadata.ProcessId;
                    current.Metadata.ProcessName = agent.Metadata.ProcessName;
                    current.Metadata.UserName = agent.Metadata.UserName;
                    current.Metadata.EndPoint = agent.Metadata.EndPoint;
                    current.Metadata.Version = agent.Metadata.Version;
                    current.Metadata.SleepInterval = agent.Metadata.SleepInterval;
                    current.Metadata.SleepJitter = agent.Metadata.SleepJitter;
                    current.LastSeen = agent.LastSeen;
                    current.Path = agent.Path;
                    current.ListenerId = agent.ListenerId;
                    current.FirstSeen = agent.FirstSeen;
                    return current;
                });

                this.AgentsUpdated?.Invoke(this, new EventArgs());
                if (isNew && !this.isSyncing)
                    this.AgentAdded?.Invoke(this, agent);

            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._agents.ContainsKey(id))
                        this._agents.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateTask(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"Tasks/{id}");

                var tr = JsonConvert.DeserializeObject<AgentTaskResponse>(response);

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
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._tasks.ContainsKey(id))
                        this._tasks.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateResult(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"Results/{id}");
                var tr = JsonConvert.DeserializeObject<AgentTaskResultResponse>(response);

                var res = new AgentTaskResult()
                {
                    Id = tr.Id,
                    Result = tr.Result,
                    Info = tr.Info,
                    Error = tr.Error,
                    Objects = tr.Objects,
                    Status = (AgentResultStatus)tr.Status,
                };

                foreach (var file in tr.Files)
                    res.Files.Add(new Models.TaskFileResult() { FileId = file.FileId, FileName = file.FileName, IsDownloaded = file.IsDownloaded });

                //new respone or response change detected

                if (!_results.ContainsKey(res.Id)) // new response
                {
                    if (res.Status == AgentResultStatus.Completed && !this.isSyncing)
                        this.TaskResultUpdated?.Invoke(this, res);
                }
                else
                {
                    //Change detected :
                    var existing = this._results[res.Id];
                    if (res.Result != existing.Result
                        || res.Error != existing.Error
                        || res.Objects != existing.Objects
                        || res.Status  != existing.Status
                        || res.Info != existing.Info
                        || res.Files.Count != existing.Files.Count
                        //                        || res.Files.Count(f => f.IsDownloaded) != existing.Files.Count(f => f.IsDownloaded)
                        )
                    {
                        if (res.Status == AgentResultStatus.Completed && !this.isSyncing)
                            this.TaskResultUpdated?.Invoke(this, res);
                    }
                }

                this._results.AddOrUpdate(tr.Id, res, (key, current) =>
                {
                    current.Result = res.Result;
                    current.Error = res.Error;
                    current.Objects = res.Objects;
                    current.Info = res.Info;
                    current.Status = res.Status;
                    current.Files.Clear();
                    foreach (var file in res.Files)
                    {
                        current.Files.Add(file);
                    }
                    return current;
                });

                var running = this._tasks.Values.Where(t => !this._results.ContainsKey(t.Id) || this._results[t.Id].Status != AgentResultStatus.Completed).ToList();
                this.RunningTaskChanged?.Invoke(this, running);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._results.ContainsKey(id))
                        this._results.Remove(id, out _);
                }
                else
                    throw e;
            }
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
            var agents = GetAgents().OrderBy(a => a.FirstSeen).ToList();
            if (index < 0)
                return null;
            if (index > agents.Count() -1)
                return null;

            return agents[index];
        }

        public Agent GetAgent(string id)
        {
            if (!this._agents.ContainsKey(id))
                return null;
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

        public void AddTask(AgentTask task)
        {
            this._tasks.AddOrUpdate(task.Id, task, (key, current) => { return current; });
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



        public async Task<HttpResponseMessage> CreateListener(string name, int port, string address, bool secured)
        {
            var requestObj = new ApiModels.Requests.StartHttpListenerRequest();
            requestObj.Name = name;
            requestObj.BindPort  = port;
            requestObj.Ip = address;
            requestObj.Secured = secured;
            var requestContent = JsonConvert.SerializeObject(requestObj);

            return await _client.PostAsync("/Listeners/", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> StopListener(string id, bool clean)
        {
            return await _client.DeleteAsync($"/Listeners/?id={id}&clean={clean}");
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
            //Console.WriteLine($"Starting Download of {id}");
            var desc = await this.SetupDownload(id);
            var chunks = new List<FileChunckResponse>();

            int progress = 0;
            for (int index = 0; index < desc.ChunkCount; ++index)
            {
                var chunk = this.GetFileChunk(desc.Id, index).Result;
                chunks.Add(chunk);

                var newprogress = index * 100 / desc.ChunkCount;
                if (progress != newprogress)
                    OnCompletionChanged?.Invoke(progress);
                progress = newprogress;
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

        public const int ChunkSize = 100000;

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
            int progress = 0;
            foreach (var chunk in chunks)
            {
                await PostFileChunk(chunk);
                var newprogress = index * 100 / desc.ChunkCount;
                if (progress != newprogress)
                    OnCompletionChanged?.Invoke(progress);
                index++;
                progress = newprogress;
            }
            OnCompletionChanged?.Invoke(100);

            return desc.Id;
        }

        public async Task<ServerConfig> ServerConfig()
        {
            var response = await _client.GetAsync($"/Config");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var conf = JsonConvert.DeserializeObject<ServerConfig>(json);
            return conf;
        }

        public async Task TaskAgentToDownloadFile(string agentId, string fileId)
        {
            var response = await _client.GetAsync($"/Agents/{agentId}/File?fileId={fileId}");


            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

        }

        #region Proxy
        public async Task<bool> StartProxy(string agentId, int port)
        {
            var resp = await _client.GetAsync($"/Agents/{agentId}/startproxy?port={port}");
            if (resp.IsSuccessStatusCode)
                return true;
            else
            {
                Terminal.WriteError(resp.StatusCode + " " + resp.ReasonPhrase);
                return false;
            }
        }
        public async Task<bool> StopProxy(string agentId)
        {
            var resp = await _client.GetAsync($"/Agents/{agentId}/stopproxy");
            if (resp.IsSuccessStatusCode)
                return true;
            else
            {
                Terminal.WriteError(resp.StatusCode + " " + resp.ReasonPhrase);
                return false;
            }
        }
        #endregion

        #region WebHost
        public async Task WebHost(string path, byte[] fileContent, bool isPowerShell, string description)
        {
            var wh = new FileWebHost()
            {
                Path = path,
                Data = fileContent,
                IsPowershell = isPowerShell,
                Description = description
            };
            var requestContent = JsonConvert.SerializeObject(wh);

            var response = await _client.PostAsync($"/WebHost", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task<List<FileWebHost>> GetWebHosts()
        {
            var response = await _client.GetAsync($"/WebHost");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<FileWebHost>>(json);
            return list;

        }

        public async Task<List<WebHostLog>> GetWebHostLogs()
        {
            var response = await _client.GetAsync($"/WebHost/Logs");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<WebHostLog>>(json);
            return list;

        }

        public async Task RemoveWebHost(string path)
        {
            var wh = new FileWebHost()
            {
                Path = path,
            };
            var requestContent = JsonConvert.SerializeObject(wh);
            var response = await _client.PostAsync($"/WebHost/Remove", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task ClearWebHosts()
        {
            var response = await _client.GetAsync($"/WebHost/Clear");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }


        #endregion
    }
}
