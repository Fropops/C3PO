using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class PrintWorkingDirectoryCommand : AgentCommand
    {
        public override string Name => "pwd";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            context.Result.Result = Directory.GetCurrentDirectory();
        }
    }
}
