using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands.Execution
{
    internal class WinRMCommand : AgentCommand
    {
        public override string Name => "winrm";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var target = task.SplittedArgs[0];
            var cmd = task.SplittedArgs[1];
            context.AppendResult($"target : {target}");
            context.AppendResult($"cmd length : {cmd.Length}");


            var uri = new Uri($"http://{target}:5985/wsman");

            WSManConnectionInfo conn = null;

            //if (task.Arguments.TryGetValue("domain", out var domain))
            //{
            //    if (task.Arguments.TryGetValue("username", out var username))
            //    {
            //        if (task.Arguments.TryGetValue("password", out var plaintext))
            //        {
            //            var credential = new PSCredential($"{domain}\\{username}", plaintext.ToSecureString());
            //            conn = new WSManConnectionInfo(uri, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", credential);
            //        }
            //    }
            //}

            conn = new WSManConnectionInfo(uri);

            using (var rs = RunspaceFactory.CreateRunspace(conn))
            {
                rs.Open();

                using (var posh = System.Management.Automation.PowerShell.Create())
                {
                    posh.Runspace = rs;
                    posh.AddScript(cmd);

                    var results = posh.Invoke();
                    var output = string.Join(Environment.NewLine, results.Select(o => o.ToString()).ToArray());

                    context.AppendResult(output);
                }
            }
        }
    }
}
