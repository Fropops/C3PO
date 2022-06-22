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
            if (task.SplittedArgs.Length != 1 || task.SplittedArgs.Length != 2)
            {
                result.Result = $"Usage : {this.Name} folder_to_create [recurse (true|false)]";
                return;
            }

            path = task.SplittedArgs[0];
            bool recurse = false;
            if (task.SplittedArgs.Length > 1)
                if (!bool.TryParse(task.SplittedArgs[1], out recurse))
                {
                    result.Result = $"Usage : {this.Name} folder_to_create [recurse (true|false)]";
                    return;
                }

            Directory.Delete(path, recurse);
            if (!Directory.Exists(path))
            {
                result.Result = $"{path} deleted";
                return;
            }

            result.Result = $"Failed to delete {path}";
        }


    }
}
