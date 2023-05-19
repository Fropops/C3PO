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
                context.Result.Result = $"Delay is {context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval}s +/- {context.Agent.Communicator.MessageService.AgentMetaData.SleepJitter*100}%";
                return;
            }

            int delay = int.Parse(task.SplittedArgs[0]);
            int jitter = 0;
            if (task.SplittedArgs.Count() > 1)
            {
                jitter = int.Parse(task.SplittedArgs[1]);
            }

            if(jitter < 0 || jitter >= 100)
            {
                context.Result.Result = "Jitter is not correct (should be 0-99%)";
            }

            context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval = delay;
            context.Agent.Communicator.MessageService.AgentMetaData.SleepJitter = jitter;

            context.Result.Result = $"Delay is set to {context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval}s - {context.Agent.Communicator.MessageService.AgentMetaData.SleepJitter}%";

            this.SendMetadataWithResult = true;
        }
    }
}
