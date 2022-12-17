using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class PathCommandOptions
    {
    }

    public class PathCommand : EnhancedCommand<PathCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Show current agent path";
        public override string Name => "path";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async override Task<bool> HandleCommand(CommandContext<PathCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if(agent == null)
                context.Terminal.WriteError("No corresponding agent !");

            var first = true;
            var path = string.Empty;
            foreach (var pathElement in agent.Path)
            {
                if (first)
                {
                    path += "TeamServer --> " + pathElement;
                    first = false;
                    continue;
                }

                path += " <-- " + pathElement;

            }

            context.Terminal.WriteLine(path);
            return true;
        }
    }
}
