using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commands
{
    public class RevertToSelfCommand : AgentCommand
    {
        public override string Name => "revert-self";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {

            /*if (Agent.Native.Advapi.RevertToSelf())
            {
                context.Result.Result += $"Reverted to self";
                return;
            }

            context.Result.Result += $"Failed to revert";
            return;*/

            if(!ImpersonationHelper.HasCurrentImpersonation)
            {
                context.Result.Result += $"No impersonation to revert";
                return;
            }

            ImpersonationHelper.Reset();
            context.Result.Result += $"Reverted to self";
        }
    }
}
