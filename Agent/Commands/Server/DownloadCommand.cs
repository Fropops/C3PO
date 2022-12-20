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
            var fileContent = file.GetFileContent();

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            context.Result.Result = $"File downloaded to {fileName}.";
        }
    }
}
