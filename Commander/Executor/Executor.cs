using ApiModels.Response;
using Commander.Commands;
using Commander.Commands.Agent;
using Commander.Commands.Listener;
using Commander.Communication;
using Commander.Models;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.IO;
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

            this.CommModule.ConnectionStatusChanged +=CommModule_ConnectionStatusChanged;
            this.CommModule.RunningTaskChanged += CommModule_RunningTaskChanged;
            this.CommModule.TaskResultUpdated += CommModule_TaskResultUpdated;
            this.CommModule.AgentsUpdated +=CommModule_AgentsUpdated;
            this.CommModule.AgentAdded +=CommModule_AgentAdded;
            //end events

            this.Terminal.NewLine(false);
        }

        private void CommModule_AgentAdded(object sender, Agent e)
        {
            Terminal.Interrupt();
            string userName = e.Metadata.UserName;
            if (e.Metadata.Integrity == "High")
                userName += "*";
            Terminal.WriteInfo($"New Agent Checking in : {userName}/{e.Metadata.Hostname} ({e.Metadata.Id.ToShortGuid()})");
            Terminal.Restore();
        }

        private void CommModule_AgentsUpdated(object sender, EventArgs e)
        {
            if (this.CurrentAgent != null)
            {
                this.CurrentAgent = this.CommModule.GetAgent(this.CurrentAgent.Metadata.Id);
            }
        }

        private void CommModule_TaskResultUpdated(object sender, AgentTaskResult res)
        {
            var task = this.CommModule.GetTask(res.Id);
            if (task == null)
            {
                task =  new AgentTask()
                {
                    Id = res.Id,
                    AgentId = this.CurrentAgent.Metadata.Id,
                    Label = "unknown task",
                    Command = "unknown",
                };
                this.CommModule.AddTask(task);
            }
            if (this.CurrentAgent == null || task.AgentId != this.CurrentAgent.Metadata.Id)
                return;

            this.Terminal.Interrupt();
            task.Print(res, this.Terminal);
            foreach(var file in res.Files.Where(f => !f.IsDownloaded))
            {
                bool first = true;
                var bytes = this.CommModule.Download(file.FileId, a =>
                {
                    this.Terminal.ShowProgress("dowloading", a, first);
                    first = false;
                }).Result;

                using (FileStream fs = new FileStream(file.FileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                this.Terminal.WriteSuccess($"File {file.FileName} successfully downloaded");
            }
            this.Terminal.Restore();
        }

        int lastRunningCount = 0;

        private void CommModule_RunningTaskChanged(object sender, List<AgentTask> tasks)
        {
            //if(this.CurrentAgent == null)
            //{
            //    if(lastRunningCount != 0)
            //    {
            //        Terminal.CanHandleInput = false;
            //        Terminal.SaveCursorPosition();
            //        Terminal.SetCursorPosition(0, 0);
            //        Terminal.DrawBackGround(TerminalConstants.DefaultBackGroundColor, lastRunningCount + 1);
            //        Terminal.ResetCursorPosition();
            //        Terminal.CanHandleInput = true;
            //    }
            //    return;
            //}
            //tasks = tasks.Where(t => t.AgentId == this.CurrentAgent.Metadata.Id).ToList();
            //if (tasks.Count == 0 && lastRunningCount == 0)
            //    return;

            //Terminal.CanHandleInput = false;

            //Terminal.SaveCursorPosition();
            //Terminal.SetCursorPosition(0, 0);
            //Terminal.DrawBackGround(TerminalConstants.DefaultBackGroundColor, lastRunningCount + 1);

            //lastRunningCount = tasks.Count;
            //if (tasks.Any())
            //{
            //    Terminal.SetCursorPosition(0, 0);
            //    Terminal.DrawBackGround(ConsoleColor.Cyan, tasks.Count + 1);

            //    Terminal.SetBackGroundColor(ConsoleColor.Cyan);
            //    Terminal.SetForeGroundColor(ConsoleColor.Black);

            //    Terminal.SetCursorPosition(0, 0);
            //    Terminal.WriteLine("Running Commands :");
            //    int index = 0;
            //    foreach (var task in tasks.OrderBy(t => t.RequestDate))
            //    {
            //        index++;
            //        int completion = 0;
            //        var res = this.CommModule.GetTaskResult(task.Id);
            //        if (res != null)
            //            completion = res.Completion;

            //        Terminal.Write($" #{index} {task.FullCommand} - {completion}%");
            //        Terminal.WriteLine();
            //    }
            //}

            //Terminal.SetForeGroundColor(TerminalConstants.DefaultForeGroundColor);
            //Terminal.SetBackGroundColor(TerminalConstants.DefaultBackGroundColor);
            //Terminal.ResetCursorPosition();

            //Terminal.CanHandleInput = true;
        }

        private void CommModule_ConnectionStatusChanged(object sender, ConnectionStatus e)
        {
            string status = string.Empty;
            this.Terminal.Interrupt();
            switch (e)
            {
                case ConnectionStatus.Connected:
                    {
                        status = $"Commander is now connected to {this.CommModule.ConnectAddress}:{this.CommModule.ConnectPort}.";
                        this.Terminal.WriteSuccess(status);
                    }
                    break;
                default:
                    {
                        status = $"Commander cannot connect to {this.CommModule.ConnectAddress}:{this.CommModule.ConnectPort}.";
                        this.Terminal.WriteError(status);
                    }
                    break;

            }

            this.Terminal.Restore();
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
                this.Terminal.WriteError(error);
                this.InputHandled(null, false);
                return;
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
