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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            throw new NotImplementedException();
            //var fileName = task.FileName;

            //var fileContent = commm.Download(task.FileId, a =>
            //{
            //    result.Info = $"Downloading {fileName} ({a}%)";
            //    commm.SendResult(result);
            //}).Result;


            //this.Notify(result, commm, $"{fileName} Downloaded");

            //using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            //{
            //    fs.Write(fileContent, 0, fileContent.Length);
            //}

            //result.Result = $"File dowloaded to {fileName}.";
        }
    }
}
