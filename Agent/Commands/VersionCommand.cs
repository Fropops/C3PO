using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class VersionCommand : AgentCommand
    {
        public override string Name => "version";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            context.Result.Result = "Version : 3.1";
        }
    }
}
