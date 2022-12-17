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
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            string path;
            if (task.SplittedArgs.Length != 1)
            {
                result.Result = $"Usage : {this.Name} folder_to_delete";
                return;
            }
                
            path = task.SplittedArgs[0];

            var dirInfo = Directory.CreateDirectory(path);
            result.Result = $"{dirInfo.FullName} created";
        }
    }
}
