using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DInvoke
{
    public static class Kernel32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;// => Marshal.SizeOf(this);
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [Flags]
        public enum PROCESS_CREATION_FLAGS : uint
        {
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SECURE_PROCESS = 0x00400000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [Flags]
        public enum HANDLE_FLAGS : uint
        {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }


        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CreateProcessW(
               [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
               [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
               ref SECURITY_ATTRIBUTES lpProcessAttributes,
               ref SECURITY_ATTRIBUTES lpThreadAttributes,
               [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
               PROCESS_CREATION_FLAGS dwCreationFlags,
               IntPtr lpEnvironment,
               [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
               ref STARTUPINFOEX lpStartupInfo,
               out PROCESS_INFORMATION lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool InitializeProcThreadAttributeList(
    IntPtr lpAttributeList,
    int dwAttributeCount,
    int dwFlags,
    ref IntPtr lpSize);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool UpdateProcThreadAttribute(
    IntPtr lpAttributeList,
    uint dwFlags,
    IntPtr attribute,
    IntPtr lpValue,
    IntPtr cbSize,
    IntPtr lpPreviousValue,
    IntPtr lpReturnSize);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate void DeleteProcThreadAttributeList(IntPtr lpAttributeList);


            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool ReadFile(
                IntPtr hFile,
                IntPtr buffer,
                uint nNumberOfBytesToRead,
                ref uint lpNumberOfBytesRead,
                IntPtr lpOverlapped);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CreatePipe(
                ref IntPtr hReadPipe,
                ref IntPtr hWritePipe,
                ref SECURITY_ATTRIBUTES lpPipeAttributes,
                uint nSize);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool SetHandleInformation(
                IntPtr hObject,
                HANDLE_FLAGS dwMask,
                HANDLE_FLAGS dwFlags);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CloseHandle(
                IntPtr hObject);


            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate IntPtr VirtualAllocEx(
               IntPtr hProcess,
               IntPtr lpAddress,
               int dwSize,
               AllocationType flAllocationType,
               MemoryProtection flProtect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool VirtualProtectEx(
           IntPtr hProcess,
           IntPtr lpAddress,
           int dwSize,
           MemoryProtection flNewProtect,
           out MemoryProtection lpflOldProtect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate bool WriteProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                byte[] lpBuffer,
                int nSize,
                out IntPtr lpNumberOfBytesWritten);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate IntPtr CreateRemoteThread(
           IntPtr hProcess,
           IntPtr lpThreadAttributes,
           uint dwStackSize,
           IntPtr lpStartAddress,
           IntPtr lpParameter,
           uint dwCreationFlags,
           out IntPtr lpThreadId);

        }


        public static bool ReadFile(IntPtr hFile, out byte[] lpBuffer, uint nNumberOfBytesToRead)
        {
            IntPtr lpOverlapped = IntPtr.Zero;
            var buff = Marshal.AllocHGlobal((int)nNumberOfBytesToRead);
            object[] parameters = { hFile, buff, (uint)nNumberOfBytesToRead, null, lpOverlapped };

            var retVal = (bool)Generic.DynamicApiInvoke(@"kernel32.dll", @"ReadFile", typeof(Delegates.ReadFile), ref parameters);

            var lpNumberOfBytesRead = (uint)parameters[3];
            lpBuffer = new byte[lpNumberOfBytesRead];
            Marshal.Copy(buff, lpBuffer, 0, (int)lpNumberOfBytesRead);

            Marshal.FreeHGlobal(buff);

            return retVal;
        }

        public static bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes)
        {
            IntPtr readPtr = IntPtr.Zero;
            IntPtr writePtr = IntPtr.Zero;
            uint nSize = 0;
            lpPipeAttributes.nLength = Marshal.SizeOf(lpPipeAttributes);
            object[] parameters = { readPtr, writePtr, lpPipeAttributes, nSize };

            var retVal = (bool)Generic.DynamicApiInvoke(@"kernel32.dll", @"CreatePipe", typeof(Delegates.CreatePipe), ref parameters);
            hReadPipe = (IntPtr)parameters[0];
            hWritePipe = (IntPtr)parameters[1];
            return retVal;
        }

        public static bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags)
        {
            object[] parameters = { hObject, dwMask, dwFlags };

            var retVal = (bool)Generic.DynamicApiInvoke(@"kernel32.dll", @"SetHandleInformation", typeof(Delegates.SetHandleInformation), ref parameters);
            return retVal;
        }

        public static bool CreateProcessW(string lpApplicationName, string lpCommandLine, uint dwCreationFlags, string lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation, bool inheritHandles = false)
        {
            var lpProcessAttributes = new SECURITY_ATTRIBUTES();
            var lpThreadAttributes = new SECURITY_ATTRIBUTES();

            object[] parameters = { lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, inheritHandles, dwCreationFlags, IntPtr.Zero, lpCurrentDirectory, lpStartupInfo, null };

            var retVal = (bool)Generic.DynamicApiInvoke("kernel32.dll", "CreateProcessW", typeof(Delegates.CreateProcessW), ref parameters);
            lpProcessInformation = (PROCESS_INFORMATION)parameters[9];
            return retVal;
        }


        public static bool InitializeProcThreadAttributeList(ref IntPtr lpAttributeList, int dwAttributeCount)
        {
            var lpSize = IntPtr.Zero;
            object[] parameters = { IntPtr.Zero, dwAttributeCount, 0, lpSize };

            var retVal = (bool)Generic.DynamicApiInvoke(@"kernel32.dll", @"InitializeProcThreadAttributeList", typeof(Delegates.InitializeProcThreadAttributeList), ref parameters);
            lpSize = (IntPtr)parameters[3];

            lpAttributeList = Marshal.AllocHGlobal(lpSize);
            parameters = new object[] { lpAttributeList, dwAttributeCount, 0, lpSize };
            retVal = (bool)Generic.DynamicApiInvoke(@"kernel32.dll", @"InitializeProcThreadAttributeList", typeof(Delegates.InitializeProcThreadAttributeList), ref parameters);

            return retVal;
        }

        public static bool UpdateProcThreadAttribute(ref IntPtr lpAttributeList, IntPtr attribute, ref IntPtr lpValue)
        {
            object[] parameters = { lpAttributeList, (uint)0, attribute, lpValue, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero };
            var retVal = (bool)Generic.DynamicApiInvoke("kernel32.dll", "UpdateProcThreadAttribute", typeof(Delegates.UpdateProcThreadAttribute), ref parameters);
            return retVal;
        }

        public static void DeleteProcThreadAttributeList(ref IntPtr lpAttributeList)
        {
            object[] parameters = { lpAttributeList };
            Generic.DynamicApiInvoke("kernel32.dll", "DeleteProcThreadAttributeList", typeof(Delegates.DeleteProcThreadAttributeList), ref parameters);
        }

        public static bool CloseHandle(IntPtr hObject)
        {
            object[] parameters = { hObject };
            var retVal = (bool)Generic.DynamicApiInvoke("kernel32.dll", "CloseHandle", typeof(Delegates.CloseHandle), ref parameters);
            return retVal;
        }

        public static IntPtr VirtualAllocEx(
             IntPtr hProcess,
             IntPtr lpAddress,
             int dwSize,
             AllocationType flAllocationType,
             MemoryProtection flProtect)
        {
            object[] parameters = { hProcess, lpAddress, dwSize, flAllocationType, flProtect };
            var retVal = (IntPtr)Generic.DynamicApiInvoke("kernel32.dll", "VirtualAllocEx", typeof(Delegates.VirtualAllocEx), ref parameters);
            return retVal;
        }

        public static bool VirtualProtectEx(IntPtr hProcess,
               IntPtr lpAddress,
               int dwSize,
               MemoryProtection flNewProtect,
               out MemoryProtection lpflOldProtect)
        {
            object[] parameters = { hProcess, lpAddress, dwSize, flNewProtect, null };
            var retVal = (bool)Generic.DynamicApiInvoke("kernel32.dll", "VirtualProtectEx", typeof(Delegates.VirtualProtectEx), ref parameters);
            lpflOldProtect = (MemoryProtection)parameters[4];
            return retVal;
        }


        public static bool WriteProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten)
        {
            object[] parameters = { hProcess, lpBaseAddress, lpBuffer, nSize, null };
            var retVal = (bool)Generic.DynamicApiInvoke("kernel32.dll", "WriteProcessMemory", typeof(Delegates.WriteProcessMemory), ref parameters);
            lpNumberOfBytesWritten = (IntPtr)parameters[4];
            return retVal;
        }

        public static IntPtr CreateRemoteThread(IntPtr hProcess,
               IntPtr lpThreadAttributes,
               uint dwStackSize,
               IntPtr lpStartAddress,
               IntPtr lpParameter,
               uint dwCreationFlags,
               out IntPtr lpThreadId)
        {
            object[] parameters = { hProcess, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, null };
            var retVal = (IntPtr)Generic.DynamicApiInvoke("kernel32.dll", "CreateRemoteThread", typeof(Delegates.CreateRemoteThread), ref parameters);
            lpThreadId = (IntPtr)parameters[6];
            return retVal;
        }
    }


}
