using ApiModels.Requests;
using ApiModels.Response;
using Commander.Models;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public event EventHandler<AgentTaskResult> TaskResultUpdated;


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
                    Arguments = tr.Arguments,
                    Command = tr.Command,
                    File = tr.File,
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
                    Completion = tr.Completion,
                    Completed = tr.Completed,
                };

                //new respone or response change detected

                if(!_results.ContainsKey(res.Id)) // new response
                {
                    if (res.Completed && !firstLoad)
                        this.TaskResultUpdated?.Invoke(this, res);
                }
                else
                {
                    //Change detected :
                    var existing = this._results[res.Id];
                    if (res.Result != existing.Result
                        || res.Completed != existing.Completed
                        || res.Completion != existing.Completion)
                    {
                        if (res.Completed && !firstLoad)
                            this.TaskResultUpdated?.Invoke(this, res);
                    }
                }

                this._results.AddOrUpdate(tr.Id, res, (key, current) =>
                {
                    current.Result = res.Result;
                    current.Completed = res.Completed;
                    current.Completion = res.Completion;
                    return current;
                });

                var running = this._tasks.Values.Where(t => !this._results.ContainsKey(t.Id) || !this._results[t.Id].Completed).ToList();
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

            this._agents.Clear();
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
                        AvailableCommands = ar.Metadata.AvailableCommands,
                    },
                    LastSeen = ar.LastSeen
                };

                this._agents.AddOrUpdate(ar.Metadata.Id, agent, (key, current) =>
                {
                    current.LastSeen = agent.LastSeen;
                    return current;
                });
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

        public IEnumerable<AgentTask> GetTasks(string id)
        {
            return this._tasks.Values.Where(t => t.AgentId == id).OrderByDescending(t => t.RequestDate);
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



        public async Task<HttpResponseMessage> TaskAgent(string id, string cmd, string parms)
        {
            var taskrequest = new TaskAgentRequest()
            {
                Command = cmd,
            };
            if (!string.IsNullOrEmpty(parms))
                taskrequest.Arguments = parms;

            var requestContent = JsonConvert.SerializeObject(taskrequest);

            return await _client.PostAsync($"/Agents/{id}", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
        }
    }
}
