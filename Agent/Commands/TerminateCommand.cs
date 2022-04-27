using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class TerminateCommand: AgentCommand
    {
        public override string Name => "terminate";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            result.Result = "Exiting...";
            Environment.Exit(0);
        }
    }
}
