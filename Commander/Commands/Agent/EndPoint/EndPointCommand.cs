﻿using Commander.Communication;
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

namespace Commander.Commands.Agent
{
    public abstract class EndPointCommand<T> : EnhancedCommand<T>
    {
        public override string Category => CommandCategory.Core;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override RootCommand Command => new RootCommand(this.Description);

        public abstract CommandId CommandId { get; }

        protected async Task CallEndPointCommand(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;
            await context.CommModule.TaskAgent(context.CommandLabel, agent.Id, this.CommandId, this.SpecifyParameters(context));

            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }

        protected virtual ParameterDictionary SpecifyParameters(CommandContext context)
        {
            return null;
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            await this.CallEndPointCommand(context);
            return true;
        }
    }

    public abstract class EndPointCommand : EndPointCommand<EmptyCommandOptions>
    {
    }
   
}
