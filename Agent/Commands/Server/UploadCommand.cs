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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            throw new NotImplementedException();
            //if (task.SplittedArgs.Length == 0)
            //{
            //    result.Result = "Please specify the name of the file to upload!";
            //    return;
            //}

            //var path = task.SplittedArgs[0];
            //var filename = Path.GetFileName(path);
           
            //if(!File.Exists(path))
            //{
            //    result.Result = $"File {path} not found.";
            //    return;
            //}

            //byte[] fileBytes = null;
            //using (FileStream fs = File.OpenRead(path))
            //{
            //    fileBytes = new byte[fs.Length];
            //    fs.Read(fileBytes, 0, (int)fs.Length);
            //}

            //if (task.SplittedArgs.Length > 1)
            //    filename = task.SplittedArgs[1];
            //else
            //    filename = Path.GetFileName(filename);

            //string fileId = commm.Upload(fileBytes, filename, a =>
            //{
            //    result.Info = $"Uploading {filename} ({a}%)";
            //    commm.SendResult(result);
            //}).Result;

            //result.Result = $"File {path} uploaded to the server.";
            //result.Files.Add(new TaskFileResult() { FileId = fileId, FileName = filename });
        }
    }
}
