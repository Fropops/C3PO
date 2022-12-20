using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class WhoamiCommand : AgentCommand
    {
        public override string Name => "whoami";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var identity = WindowsIdentity.GetCurrent();
            context.Result.Result = identity.Name;
        }
    }
}
