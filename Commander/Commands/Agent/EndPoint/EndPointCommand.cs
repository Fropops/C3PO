using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Shared;
using BinarySerializer;

namespace Commander.Commands.Agent
{
    public abstract class EndPointCommand<T> : EnhancedCommand<T>
    {
        public override string Category => CommandCategory.Core;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override RootCommand Command => new RootCommand(this.Description);

        public abstract CommandId CommandId { get; }

        protected async Task CallEndPointCommand(CommandContext<T> context)
        {
            var agent = context.Executor.CurrentAgent;
            await context.CommModule.TaskAgent(context.CommandLabel, agent.Id, this.CommandId, context.Parameters);

            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }

        protected virtual async Task<bool> CheckParams(CommandContext<T> context)
        {
            return true;
        }

      
        protected virtual void SpecifyParameters(CommandContext<T> context)
        {
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            if (!await this.CheckParams(context))
                return false;
            this.SpecifyParameters(context);
            await this.CallEndPointCommand(context);
            return true;
        }
    }

    public abstract class EndPointCommand : EndPointCommand<EmptyCommandOptions>
    {
    }
   
}
