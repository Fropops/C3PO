using Commander.Communication;
using Commander.Executor;
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
    public class ListAgentsCommandOptions
    {
        public bool verbose { get; set; }
    }

    public class ListAgentsCommand : EnhancedCommand<ListAgentsCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all agents";
        public override string Name => "list";

        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ListAgentsCommandOptions> context)
        {
            var results = new SharpSploitResultList<ListAgentResult>();

            var result = context.CommModule.GetAgents();


            if (result.Count() == 0)
            {
                context.Terminal.WriteLine("No Agents running.");
                return true;
            }

            

            var index = 0;
            foreach (var agent in result)
            {
                results.Add(new ListAgentResult()
                {
                    Index = index,
                    Id = agent.Metadata.Id,
                    LastSeen = agent.LastSeen.ToLocalTime(),
                    Actif = agent.LastSeen.AddSeconds(30) > DateTime.UtcNow ? "Yes" : "No",
                    UserName = agent.Metadata.UserName,
                    HostName = agent.Metadata.Hostname,
                    Integrity = agent.Metadata.Integrity,
                    Process = agent.Metadata.ProcessName,
                });
                index++;
            }

            context.Terminal.WriteLine(results.ToString());

            return true;
        }

        public sealed class ListAgentResult : SharpSploitResult
        {
            public int Index { get; set; }
            public string Id { get; set; }

            public DateTime LastSeen { get; set; }
            public string UserName { get; set; }
            public string HostName { get; set; }

            public string Process { get; set; }

            public string Integrity { get; set; }

            public string Actif { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Index), Value = Index },
                new SharpSploitResultProperty { Name = nameof(Actif), Value = Actif },
                new SharpSploitResultProperty { Name = nameof(Process), Value = Process },
                new SharpSploitResultProperty { Name = nameof(Integrity), Value = Integrity },
                new SharpSploitResultProperty { Name = nameof(UserName), Value = UserName },
                new SharpSploitResultProperty { Name = nameof(HostName), Value = HostName },
                new SharpSploitResultProperty { Name = nameof(LastSeen), Value = LastSeen },
                new SharpSploitResultProperty { Name = nameof(Id), Value = Id },
            };
        }

    }


}
