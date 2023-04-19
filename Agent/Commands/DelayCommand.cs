using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DelayCommand : AgentCommand
    {
        public override string Name => "delay";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Count() == 0)
            {
                context.Result.Result = $"Duration is mandatory";
                return;
            }

            int delay = int.Parse(task.SplittedArgs[0]);
            Thread.Sleep(delay*1000);
        }
    }
}
