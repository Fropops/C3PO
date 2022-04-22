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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length == 0)
            {
                result.Result = "Please specify the name of the file to download!";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Completion = a;
                commm.SendResult(result);
            }).Result;

            string path = string.Empty;
            if (task.SplittedArgs.Length > 1)
            {
                path = task.SplittedArgs[1];
            }
            else
            {
                path = Path.Combine(Environment.CurrentDirectory, fileName);
            }

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            result.Result = $"File dowloaded to {path}.";
        }
    }
}
