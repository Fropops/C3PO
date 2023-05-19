using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Token
{
    internal class Program
    {
        const string targetPath = @"\\corp-dc.corp.local\c$";
        
        static void Main(string[] args)
        {
            //WithoutThread();
            DoWork();

            var token = Impersonate();

            using (var context = WindowsIdentity.Impersonate(token))
            {
                var thread = new Thread(DoWork);
                thread.Start();

                thread.Join();
            }
        }

        private static IntPtr Impersonate()
        {
            Console.Write(@"Creating token... ");
            IntPtr hToken = IntPtr.Zero;
            //make token
            var success = Advapi.LogonUserA(
                "nlamb",
                "CORP.LOCAL",
                "F3rrari",
                Advapi.LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS,
                Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT,
                ref hToken);

            if (success) Console.WriteLine("Success.");
            else throw new Win32Exception(Marshal.GetLastWin32Error());

            Console.Write(@"Impersonating token... ");

            // impersonate token
            success = Advapi.ImpersonateLoggedOnUser(hToken);

            if (success) Console.WriteLine("Success.");
            else throw new Win32Exception(Marshal.GetLastWin32Error());

            return hToken;
        }

        private static void DoWork()
        {
            IEnumerable<string> entries;

            //using (var identity = WindowsIdentity.GetCurrent())
            //{
            //    Console.Write($"Accessing share as {identity.Name}... ");
            //}

            try
            {
                // this will throw an unauthorized access exception
                entries = Directory.EnumerateFileSystemEntries(targetPath);

                foreach (var entry in entries)
                    Console.WriteLine(entry);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Failed.");
                Console.WriteLine(e.Message);
                Console.WriteLine();
            }

        }


        private static void WithoutThread()
        {
            
            IEnumerable<string> entries;

            using (var identity = WindowsIdentity.GetCurrent())
            {
                Console.Write($"Accessing share as {identity.Name}... ");
            }

            try
            {
                // this will throw an unauthorized access exception
                entries = Directory.EnumerateFileSystemEntries(targetPath);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Failed.");
                Console.WriteLine(e.Message);
                Console.WriteLine();
            }

            Console.Write(@"Making token for corp.local\nlamb... ");

            IntPtr hToken = IntPtr.Zero;
            // make token
            var success = Advapi.LogonUserA(
                "nlamb",
                "CORP.LOCAL",
                "F3rrari",
                Advapi.LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS,
                Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT,
                ref hToken);

            if (success) Console.WriteLine("Success.");
            else throw new Win32Exception(Marshal.GetLastWin32Error());

            Console.Write(@"Impersonating token... ");

            // impersonate token
            success = Advapi.ImpersonateLoggedOnUser(hToken);

            if (success) Console.WriteLine("Success.");
            else throw new Win32Exception(Marshal.GetLastWin32Error());

            Console.WriteLine("Accessing share again:");
            Console.WriteLine();

            // list again and it should work
            entries = Directory.EnumerateFileSystemEntries(targetPath);

            foreach (var entry in entries)
                Console.WriteLine(entry);
        }

        
    }
}
