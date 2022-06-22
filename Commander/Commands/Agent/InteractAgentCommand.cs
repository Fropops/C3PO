using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class InteractAgentCommandOptions
    {
        public int index { get; set; }

    }


    public class InteractAgentCommand : EnhancedCommand<InteractAgentCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Select an agent to interact with";
        public override string Name => "interact";
        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int>("index", "index of the agent"),
            };

        protected override async Task<bool> HandleCommand(CommandContext<InteractAgentCommandOptions> context)
        {
            var agent = context.CommModule.GetAgent(context.Options.index);
            
            if(agent == null)
            {
                context.Terminal.WriteError($"No agent with index {context.Options.index} found.");
                return false;
            }

            context.Executor.CurrentAgent = agent;
            context.Executor.Mode = ExecutorMode.AgentInteraction;

            context.Terminal.Prompt = $"${ExecutorMode.Agent} {agent.Metadata.UserName}@{agent.Metadata.Hostname}> ";

            return true;
        }
    }



}
