using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class HelpCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Give info on available commands";
        public override string Name => "help";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(CommandContext context)
        {
            var results = new SharpSploitResultList<HelpResult>();

            var mode = context.Executor.Mode;

            List<ExecutorCommand> cmds = new List<ExecutorCommand>();
            cmds.AddRange(context.Executor.GetCommandsInMode(mode));
            cmds.AddRange(context.Executor.GetCommandsInMode(ExecutorMode.All));

            context.Terminal.WriteLine("Available commands :");
            bool first = true;
            foreach (var cat in CommandCategory.All)
            {
                var tmpCmds = cmds.Where(c => c.Category == cat);
                if (!tmpCmds.Any())
                    continue;
                if (first)
                    first = false;
                else
                    context.Terminal.WriteLine();
                context.Terminal.WriteLine(" " + cat + " ");
                context.Terminal.WriteLine(string.Empty.PadLeft(cat.Length + 2, '='));

                foreach (var cmd in tmpCmds.OrderBy(c => c.Name))
                {
                    results.Add(new HelpResult()
                    {
                        Name = cmd.Name,
                        Description = cmd.Description ?? string.Empty,
                    });
                }

                context.Terminal.WriteLine(results.ToString());
                results.Clear();
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
