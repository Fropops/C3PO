using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class CatCommand : AgentCommand
    {
        public override string Name => "cat";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            string path;
            if (task.SplittedArgs.Length != 1)
            {
                result.Result = $"Usage : {this.Name} file_to_display";
                return;
            }

            path = task.SplittedArgs[0];
          
            if(!File.Exists(path))
            {
                result.Result = $"Failed to delete {path}";
                return;
            }

            if (!File.Exists(path))
            {
                result.Result = $"{path} not found";
                return;
            }

            string text = System.IO.File.ReadAllText(path);
            result.Result = text;
        }


    }
}
