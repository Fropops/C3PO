using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DeleteDirectoryCommand : AgentCommand
    {
        public override string Name => "rmdir";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            string path;
            if (task.SplittedArgs.Length != 1)
            {
                context.Result.Result = $"Usage : {this.Name} folder_to_delete";
                return;
            }
                
            path = task.SplittedArgs[0];

            var dirInfo = Directory.CreateDirectory(path);
            context.Result.Result = $"{dirInfo.FullName} created";
        }
    }
}
