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
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            string path;
            if (task.SplittedArgs.Length != 1)
            {
                context.Result.Result = $"Usage : {this.Name} file_to_display";
                return;
            }

            path = task.SplittedArgs[0];
          
            if(!File.Exists(path))
            {
                context.Result.Result = $"Failed to delete {path}";
                return;
            }

            if (!File.Exists(path))
            {
                context.Result.Result = $"{path} not found";
                return;
            }

            string text = System.IO.File.ReadAllText(path);
            context.Result.Result = text;
        }


    }
}
