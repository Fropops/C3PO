using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;

namespace Commander.Commands.Agent.EndPoint
{
    public abstract class SimpleEndPointCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Core;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public abstract CommandId CommandId { get; }
        protected override void InnerExecute(CommandContext context)
        {
            this.CallEndPointCommand(context);
        }

        protected void CallEndPointCommand(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;
            context.CommModule.TaskAgent(context.CommandLabel, agent.Id, this.CommandId, this.SpecifyParameters(context)).Wait();

            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }

        protected virtual ParameterDictionary SpecifyParameters(CommandContext context)
        {
            return null;
        }
    }
}
