using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class RunCommand : AgentCommand
    {
        public override string Name => "run";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var filename = task.SplittedArgs[0];
            string args = task.Arguments.Substring(filename.Length, task.Arguments.Length - filename.Length).Trim();
            context.Result.Result = Executor.ExecuteCommand(filename, args);
        }
    }
}
