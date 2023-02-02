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
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Agent.Helpers;
using Agent.Service;
using Agent.Commands.Services;

namespace Agent.Commands
{
    public class ProxyCommand : AgentCommand
    {
        public override string Name => "proxy";


        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var proxyService = ServiceProvider.GetService<IProxyService>();
            if (task.SplittedArgs[0] == ServiceVerbs.ShowVerb)
            {
                if (proxyService.Status == RunningService.RunningStatus.Running)
                    context.Result.Result = "Proxy is running!";
                else
                    context.Result.Result = "Proxy is stopped!";
                return;
            }


            if (task.SplittedArgs[0] == ServiceVerbs.StartVerb)
            {
                if (proxyService.Status == RunningService.RunningStatus.Running)
                {
                    context.Result.Result = "Proxy is already running!";
                    return;
                }

                proxyService.Start();
                context.Result.Result = $"Proxy started";

            }

            if (task.SplittedArgs[0] == ServiceVerbs.StopVerb)
            {
                if (proxyService.Status != RunningService.RunningStatus.Running)
                {
                    context.Result.Result = "Proxy is not running!";
                    return;
                }

                proxyService.Stop();
                context.Result.Result = $"Proxy stoped";
            }
        }
    }
}
