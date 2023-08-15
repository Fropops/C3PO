using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class EchoCommand : AgentCommand
    {
        public override string Name => "echo";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            context.Result.Result = task.Arguments;
        }
    }
}
