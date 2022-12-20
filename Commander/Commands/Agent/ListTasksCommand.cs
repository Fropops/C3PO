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
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all task of an agent, or the detail of a specific task";
        public override string Name => "view";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int?>("index", "index of the task to view"),
                new Option<int?>(new[] { "--top", "-t" }, "The max number of taks retrieved (only when no index is set. Default is 10."),
                //new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ViewTasksCommandOptions> context)
        {
            var results = new SharpSploitResultList<ViewTaskResult>();


            if (context.Options.index.HasValue)
            {
                //Show Result of the Task
                var task = context.CommModule.GetTasks(context.Executor.CurrentAgent.Metadata.Id).Skip(context.Options.index.Value).FirstOrDefault();
                if(task == null)
                {
                    context.Terminal.WriteError($"No task at index {context.Options.index}");
                    return true;
                }

                var result = context.CommModule.GetTaskResult(task.Id);
                if (result == null)
                {
                    context.Terminal.WriteInfo($"Task : {task.Command} is queued.");
                }
                else
                {
                    task.Print(result, context.Terminal);
                }
                
                
                return true;
            }
            else
            {
                int take = context.Options.Top ?? 10;
                var tasks = context.CommModule.GetTasks(context.Executor.CurrentAgent.Metadata.Id).Take(take);

                if (tasks.Count() == 0)
                {
                    context.Terminal.WriteInfo("No Tasks.");
                    return true;
                }

                var index = 0;
                foreach (var task in tasks)
                {
                    var result = context.CommModule.GetTaskResult(task.Id);

                    results.Add(new ViewTaskResult()
                    {
                        Index = index,
                        Id = task.Id,
                        Command = task.Label ?? string.Empty,
                        //Arguments = task.Arguments,
                        Info = result == null ? string.Empty : result.Info ?? string.Empty,
                        Status = result == null ? AgentResultStatus.Queued.ToString() : result.Status.ToString(),
                    });
                    index++;
                }

                context.Terminal.WriteLine(results.ToString());

                return true;
            }
           
        }

        public sealed class ViewTaskResult : SharpSploitResult
        {
            public int Index { get; set; } = 0;
            public string Id { get; set; } = string.Empty;
            public string Command { get; set; } = string.Empty;
            //public string Arguments { get; set; }
            public string Status { get; set; } = string.Empty;

            public string Info { get; set; } = string.Empty;

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
