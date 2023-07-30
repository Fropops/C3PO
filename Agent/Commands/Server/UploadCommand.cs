using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class UploadCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Upload;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.File);
            task.ThrowIfParameterMissing(ParameterId.Name);

            string fileName = task.GetParameter<string>(ParameterId.Name);
            var fileContent = task.GetParameter(ParameterId.File);

           
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            context.AppendResult($"File uploaded to {fileName}.");


            /*if (!File.Exists(path))
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
            });*/


        }
    }
}
