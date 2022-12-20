using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ShellCommand : AgentCommand
    {
        public override string Name => "shell";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            //cmd.exe /c <command>
            context.Result.Result = Executor.ExecuteCommand(@"c:\windows\system32\cmd.exe", $"/c {task.Arguments}");

        }
    }
}
