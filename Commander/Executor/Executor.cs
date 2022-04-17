using ApiModels.Response;
using Commander.Commands;
using Commander.Commands.Agent;
using Commander.Commands.Listener;
using Commander.Communication;
using Commander.Models;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Commander.Executor
{
   
    public class Executor : IExecutor
    {    
        public ExecutorMode Mode { get; set; } = ExecutorMode.None;

        public bool IsRunning
        {
            get
            {
                return !this._tokenSource.IsCancellationRequested;
            }
        }

        public Agent CurrentAgent { get; set; }
        private ICommModule CommModule { get; set; }
        public ITerminal Terminal { get; set; }

        private Dictionary<ExecutorMode, List<ExecutorCommand>> _commands = new Dictionary<ExecutorMode, List<ExecutorCommand>>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public bool IsBusy { get; private set; }

        public Executor(ITerminal terminal, ICommModule commModule)
        {
            this.CommModule = commModule;
            this.Terminal = terminal;

            this.LoadCommands();

            //suscribe to events
            this.Terminal.InputValidated += Instance_InputValidated;

            this.Terminal.NewLine(false);
        }
       

        private void Instance_InputValidated(object sender, string e)
        {
            string command = string.Empty;
            string parms = string.Empty;

            int limitIndex = e.IndexOf(' ');
            if (limitIndex == -1)
                command = e;
            else
            {
                command = e.Substring(0, limitIndex);
                parms = e.Substring(limitIndex+1, e.Length - limitIndex - 1);
            }

            this.HandleInput(command, parms);
        }

        private void PrintHeader()
        {
            this.Terminal.WriteLine("C2Sharp Commander!");
        }

        public void LoadCommands()
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(ExecutorCommand)) && !type.IsAbstract)
                {
                    var instance = Activator.CreateInstance(type) as ExecutorCommand;
                    if (!this._commands.ContainsKey(instance.AvaliableIn))
                    {
                        var list = new List<ExecutorCommand>() { instance };
                        this._commands.Add(instance.AvaliableIn, list);
                    }
                    else
                    {
                        this._commands[instance.AvaliableIn].Add(instance);
                    }
                }
            }
        }

        public IEnumerable<ExecutorCommand> GetCommandsInMode(ExecutorMode mode)
        {
            return this._commands[mode];
        }

        public ExecutorCommand GetCommandInMode(ExecutorMode mode, string commandName)
        {
            if (!this._commands.ContainsKey(mode))
            {
                return null;
            }

            var list = this._commands[mode];
            if (list == null || list.Count == 0)
            {
                return null;
            }

            var command = list.FirstOrDefault(c => c.Name == commandName);

            if (command is null)
            {
                return null;
            }

            return command;
        }

        public void HandleInput(string input, string parms)
        {
            this.Terminal.CanHandleInput = false;
            string error = $"Command {input} is unknow.";

            var cmd = this.GetCommandInMode(this.Mode, input);
            if (cmd == null)
                cmd = this.GetCommandInMode(ExecutorMode.All, input);

            if (cmd is null)
            {
                if (Mode == ExecutorMode.AgentInteraction && this.CurrentAgent.Metadata.AvailableCommands != null && this.CurrentAgent.Metadata.AvailableCommands.Any(c => c == input))
                {
                    cmd = new AgentTaskCommand(input);
                }
                else
                {
                    this.Terminal.WriteError(error);
                    this.InputHandled(null, false);
                    return;
                }
            }

            cmd.Execute(parms);
        }

        public void InputHandled(ExecutorCommand cmd, bool cmdResult)
        {
            this.Terminal.CanHandleInput = true;
            this.Terminal.NewLine(false);
        }

        public void Start()
        {
            this.Terminal.Start();
            this.CommModule.Start();
        }

        public void Stop()
        {
            this._tokenSource.Cancel();
            this.CommModule.Stop();
            this.Terminal.stop();
        }

    }
}
