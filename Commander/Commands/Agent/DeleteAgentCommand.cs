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
    public class DeleteAgentCommandOptions
    {
        public string id { get; set; }

        public bool all { get; set; }
    }


    public class DeleteAgentCommand : EnhancedCommand<DeleteAgentCommandOptions>
    {
        public override string Description => "Delete an agent from the list";
        public override string Name => "delete";
        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("id", () => "", "Id of the agent"),
                new Option(new[] { "--all", "-a" }, "Delete all the agents."),
            };

        protected override async Task<bool> HandleCommand(DeleteAgentCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            bool cmdRes = true;
            var agents = new List<Models.Agent>();
            if (options.all)
            {
                agents.AddRange(comm.GetAgents());
            }
            else
            {
                agents.Add(comm.GetAgent(options.id));
            }

            foreach (var agent in agents)
            {
                var result = await comm.StopAgent(agent.Metadata.Id);
                
                if (!result.IsSuccessStatusCode)
                {
                    terminal.WriteError($"An error occured : {result.StatusCode}");
                    cmdRes = false;
                }
                else
                    terminal.WriteSuccess($"{agent.Metadata.Id} was deleted.");
            }

            return cmdRes;
        }
    }



}
