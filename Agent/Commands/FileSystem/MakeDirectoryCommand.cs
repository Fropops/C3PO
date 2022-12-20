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
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            string path;
            if (task.SplittedArgs.Length != 1 || task.SplittedArgs.Length != 2)
            {
                context.Result.Result = $"Usage : {this.Name} folder_to_create [recurse (true|false)]";
                return;
            }

            path = task.SplittedArgs[0];
            bool recurse = false;
            if (task.SplittedArgs.Length > 1)
                if (!bool.TryParse(task.SplittedArgs[1], out recurse))
                {
                    context.Result.Result = $"Usage : {this.Name} folder_to_create [recurse (true|false)]";
                    return;
                }

            Directory.Delete(path, recurse);
            if (!Directory.Exists(path))
            {
                context.Result.Result = $"{path} deleted";
                return;
            }

            context.Result.Result = $"Failed to delete {path}";
        }


    }
}
