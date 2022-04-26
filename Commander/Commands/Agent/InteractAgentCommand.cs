﻿using ApiModels.Response;
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
        public bool verbose { get; set; }

    }


    public class InteractAgentCommand : EnhancedCommand<InteractAgentCommandOptions>
    {
        public override string Description => "Select an agent to interact with";
        public override string Name => "interact";
        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int>("index", "index of the agent"),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(InteractAgentCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var agent = comm.GetAgent(options.index);
            
            if(agent == null)
            {
                terminal.WriteError($"No agent with index {options.index} found.");
                return false;
            }

            executor.CurrentAgent = agent;
            executor.Mode = ExecutorMode.AgentInteraction;

            terminal.Prompt = $"${ExecutorMode.Agent} {agent.Metadata.UserName}@{agent.Metadata.Hostname}> ";

            return true;
        }
    }



}