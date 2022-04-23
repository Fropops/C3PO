using Agent.Commands;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media
{
    public class CaptureCommand : AgentCommand
    {
        public override string Name => "capture";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            result.Result = "Test capture command!";
        }
    }
}
