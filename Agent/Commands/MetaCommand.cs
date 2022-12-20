using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class MetaCommand : AgentCommand
    {
        public override string Name => "meta";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            context.MessageService.SendResult(context.Result, true);
        }
    }
}
