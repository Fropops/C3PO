using ApiModels.Response;
using Commander.Commands;
using Commander.Commands.Agent;
using Commander.Commands.Listener;
using Commander.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Commander
{
    public enum ExecutorMode
    {
        None,
        Listener,
        Agent,
        AgentInteraction,
        All,
    }
    public class Executor
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
        public ApiCommModule CommModule { get; set; }

        private Dictionary<ExecutorMode, List<ExecutorCommand>> _commands = new Dictionary<ExecutorMode, List<ExecutorCommand>>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public bool IsBusy { get; private set; }

        public const string DefaultPrompt = "$> ";
        public void Init(ApiCommModule module)
        {
            Terminal.Instance.InputValidated += Instance_InputValidated;
            this.CommModule = module;
            this.LoadCommands();

            this.PrintHeader();
;
            Terminal.Instance.Prompt = DefaultPrompt;
            Terminal.Instance.NewLine(false);
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

        public void PrintHeader()
        {
            Terminal.WriteLine("C2Sharp Commander!");
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
            Terminal.Instance.CanHandleInput = false;
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
                    Terminal.WriteError(error);
                    this.InputHandled(null, false);
                    return;
                }
            }

            cmd.Execute(this, parms);
        }

        public void InputHandled(ExecutorCommand cmd, bool cmdResult)
        {
            Terminal.Instance.CanHandleInput = true;
            Terminal.Instance.NewLine(false);
        }

        public void Start()
        {
            Terminal.Instance.Start();
            this.CommModule.Start();
        }

        public void Stop()
        {
            this._tokenSource.Cancel();
            this.CommModule.Stop();
            Terminal.Instance.stop();
        }

        public void SetPrompt(string prompt)
        {
            Terminal.Instance.Prompt = prompt;
        }


    }
}
