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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            if (task.SplittedArgs.Count() == 0)
            {
                result.Result = $"Delay is {agent.HttpCommunicator.Interval/1000}s +/- {agent.HttpCommunicator.Jitter*100}%";
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
                result.Result = "Jitter is not correct (should be 0-99%)";
            }

         
            delay = delay * 1000;
            agent.HttpCommunicator.Interval = delay;
            agent.HttpCommunicator.Jitter = jitter;

            agent.PipeCommunicator.Interval = delay;
            agent.PipeCommunicator.Jitter = jitter;

            result.Result = $"Delay set to {delay/1000}s +/- {jitter*100}%";
        }
    }
}
