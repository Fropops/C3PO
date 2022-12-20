using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class UnLinkCommand : AgentCommand
    {
        public override string Name => "unlink";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Count() != 1)
            {
                return;
            }

            var link = context.Agent.PipeCommunicator.Links.FirstOrDefault(l => l.AgentId == task.SplittedArgs[0]);
            if (link == null)
                context.Result.Result += "Unable to find link for " + link.AgentId;
            context.Agent.PipeCommunicator.Links.Remove(link);
        }
    }
}
