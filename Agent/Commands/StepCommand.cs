using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class StepCommand : AgentCommand
    {
        public override string Name => "step";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            context.Result.Result = $"[>] {task.Arguments}...";
            this.Notify(context, task.Arguments);
        }
    }
}
