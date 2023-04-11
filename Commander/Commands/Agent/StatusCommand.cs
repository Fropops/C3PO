using Commander.Communication;
using Commander.Executor;
using Commander.Models;
using Commander.Terminal;
using Spectre.Console;
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
        public override string Category => CommandCategory.Commander;
        public override string Description => "Show current agent status";
        public override string Name => "status";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async override Task<bool> HandleCommand(CommandContext<StatusCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (agent.IsActive == true)
                context.Terminal.WriteSuccess($"Agent {agent.Metadata.Id} is up and running !");
            else
                context.Terminal.WriteError($"Agent {agent.Metadata.Id} seems to be not responding!");

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Item").LeftAligned());
            table.AddColumn(new TableColumn("Value").LeftAligned());
            table.HideHeaders();

            table.AddRow("Id", agent.Metadata?.Id ?? string.Empty);
            table.AddRow("Hostname", agent.Metadata?.Id ?? string.Empty);
            table.AddRow("User Name", agent.Metadata?.UserName ?? string.Empty);
            table.AddRow("Process Id", agent.Metadata?.ProcessId.ToString());
            table.AddRow("Process Name", agent.Metadata?.ProcessName ?? string.Empty);
            table.AddRow("Architecture", agent.Metadata?.Architecture ?? string.Empty);
            table.AddRow("Integrity", agent.Metadata?.Integrity ?? string.Empty);
            table.AddRow("EndPoint", agent.Metadata?.EndPoint ?? string.Empty);
            table.AddRow("Version", agent.Metadata?.Version ?? string.Empty);
            table.AddRow("Sleep", agent.Metadata?.Sleep ?? string.Empty);
            table.AddRow("Last Seen", Math.Round(agent.LastSeenDelta.TotalSeconds,2).ToString() + "s" ?? string.Empty);
            

            context.Terminal.Write(table);
            return true;
        }


        
    }
}
