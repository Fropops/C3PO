using Commander.Communication;
using Commander.Executor;
using Commander.Models;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class ViewTasksCommandOptions
    {
        public int? index { get; set; }
        public int? Top { get; set; }
        public bool verbose { get; set; }
    }

    public class ViewTasksCommand : EnhancedCommand<ViewTasksCommandOptions>
    {
        public override string Description => "List all task of an agent, or the detail of a specific task";
        public override string Name => "view";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int?>("index", "index of the task to view"),
                new Option<int?>(new[] { "--top", "-t" }, "The max number of taks retrieved (only when no index is set. Default is 10."),
                //new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(ViewTasksCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var results = new SharpSploitResultList<ViewTaskResult>();


            if (options.index.HasValue)
            {
                //Show Result of the Task
                var task = comm.GetTasks(executor.CurrentAgent.Metadata.Id).Skip(options.index.Value).FirstOrDefault();
                if(task == null)
                {
                    terminal.WriteError($"No task at index {options.index}");
                    return true;
                }

                var result = comm.GetTaskResult(task.Id);
                if (result == null)
                {
                    terminal.WriteInfo($"Task : {task.Command} is queued.");
                }
                else
                {
                    task.Print(result, terminal);
                }
                
                
                return true;
            }
            else
            {
                int take = options.Top ?? 10;
                var tasks = comm.GetTasks(executor.CurrentAgent.Metadata.Id).Take(take);

                if (tasks.Count() == 0)
                {
                    terminal.WriteInfo("No Tasks.");
                    return true;
                }

                var index = 0;
                foreach (var task in tasks)
                {
                    var result = comm.GetTaskResult(task.Id);

                    results.Add(new ViewTaskResult()
                    {
                        Index = index,
                        Id = task.Id,
                        Command = task.FullCommand,
                        //Arguments = task.Arguments,
                        Info = result == null ? string.Empty : result.Info ?? string.Empty,
                        Status = result == null ? AgentResultStatus.Queued.ToString() : result.Status.ToString(),
                    }); ;
                    index++;
                }

                terminal.WriteLine(results.ToString());

                return true;
            }
           
        }

        public sealed class ViewTaskResult : SharpSploitResult
        {
            public int Index { get; set; }
            public string Id { get; set; }
            public string Command { get; set; }
            //public string Arguments { get; set; }
            public string Status { get; set; }

            public string Info { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Index), Value = Index },
                //new SharpSploitResultProperty { Name = nameof(Id), Value = Id },
                new SharpSploitResultProperty { Name = nameof(Command), Value = Command },
                //new SharpSploitResultProperty { Name = nameof(Arguments), Value = Arguments },
                new SharpSploitResultProperty { Name = nameof(Status), Value = Status },
                new SharpSploitResultProperty { Name = nameof(Info), Value = Info },
            };
        }

    }


}
