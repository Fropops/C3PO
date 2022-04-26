﻿using Commander.Communication;
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
    public class StatusCommandOptions
    {
    }

    public class StatusCommand : EnhancedCommand<StatusCommandOptions>
    {
        public override string Description => "Show current agent status";
        public override string Name => "status";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async override Task<bool> HandleCommand(StatusCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var agent = executor.CurrentAgent;
            if (agent.LastSeen.AddSeconds(30) > DateTime.UtcNow)
                terminal.WriteSuccess($"Agent {agent.Metadata.Id} is up and running !");
            else
                terminal.WriteError($"Agent {agent.Metadata.Id} seems to be not responding!");
            var results = new SharpSploitResultList<StatusResult>();
            results.Add(new StatusResult() { Name = "Id", Value = agent.Metadata.Id });
            results.Add(new StatusResult() { Name = "Hostname", Value = agent.Metadata.Hostname });
            results.Add(new StatusResult() { Name = "UserName", Value = agent.Metadata.UserName });
            results.Add(new StatusResult() { Name = "ProcessId", Value = agent.Metadata.ProcessId.ToString() });
            results.Add(new StatusResult() { Name = "ProcessName", Value = agent.Metadata.ProcessName });
            results.Add(new StatusResult() { Name = "Integrity", Value = agent.Metadata.Integrity });
            results.Add(new StatusResult() { Name = "Last Seen", Value = agent.LastSeen.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss") });
            terminal.WriteLine(results.ToString());
            return true;
        }
    }

    public sealed class StatusResult : SharpSploitResult
    {
        public string Name { get; set; }
        public string Value { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Value), Value = Value },
            };
    }
}
