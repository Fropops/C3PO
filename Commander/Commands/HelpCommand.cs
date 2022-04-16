using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class HelpCommand : ExecutorCommand
    {
        public override string Description => "Give info on available commands";
        public override string Name => "help";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(Executor executor, string parms)
        {
            var results = new SharpSploitResultList<HelpResult>();

            var mode = executor.Mode;

            List<ExecutorCommand> cmds = new List<ExecutorCommand>();
            cmds.AddRange(executor.GetCommandsInMode(mode));
            cmds.AddRange(executor.GetCommandsInMode(ExecutorMode.All));


            foreach (var cmd in cmds.OrderBy(c => c.Name))
            {
                if (cmd.Name == "dummyTask")
                    continue;
                results.Add(new HelpResult()
                {
                    Name = cmd.Name,
                    Description = cmd.Description ?? string.Empty,
                });
            }

            Terminal.WriteLine("Available commands :");
            Terminal.WriteLine(results.ToString());

            if (mode == ExecutorMode.AgentInteraction)
            {
                results.Clear();
                foreach(var cmd in executor.CurrentAgent.Metadata.AvailableCommands)
                {
                    results.Add(new HelpResult()
                    {
                        Name = cmd,
                        Description = string.Empty,
                    });
                }
                Terminal.WriteLine();
                Terminal.WriteLine("Commands on the agent :");
                Terminal.WriteLine(results.ToString());
            }

            
        }

        public sealed class HelpResult : SharpSploitResult
        {
            public string Name { get; set; }
            public string Description { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Description), Value = Description },
            };
        }
    }
}
