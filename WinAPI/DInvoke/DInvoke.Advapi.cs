using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DInvoke.Kernel32;

namespace DInvoke
{
    public static class Advapi
    {
        public enum DesiredAccess : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            TOKEN_ASSIGN_PRIMARY = 0x0001,
            TOKEN_DUPLICATE = 0x0002,
            TOKEN_IMPERSONATE = 0x0004,
            TOKEN_QUERY = 0x0008,
            TOKEN_QUERY_SOURCE = 0x0010,
            TOKEN_ADJUST_PRIVILEGES = 0x0020,
            TOKEN_ADJUST_GROUPS = 0x0040,
            TOKEN_ADJUST_DEFAULT = 0x0080,
            TOKEN_ADJUST_SESSIONID = 0x0100,
            TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY),

            TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID)
        }

        public enum TokenAccess : uint
        {
            TOKEN_ASSIGN_PRIMARY = 0x0001,
            TOKEN_DUPLICATE = 0x0002,
            TOKEN_IMPERSONATE = 0x0004,
            TOKEN_QUERY = 0x0008,
            TOKEN_QUERY_SOURCE = 0x0010,
            TOKEN_ADJUST_PRIVILEGES = 0x0020,
            TOKEN_ADJUST_GROUPS = 0x0040,
            TOKEN_ADJUST_DEFAULT = 0x0080,
            TOKEN_ADJUST_SESSIONID = 0x0100,
            TOKEN_ALL_ACCESS_P = 0x000F00FF,
            TOKEN_ALL_ACCESS = 0x000F01FF,
            TOKEN_READ = 0x00020008,
            TOKEN_WRITE = 0x000200E0,
            TOKEN_EXECUTE = 0x00020000
        }

        public enum TokenType
        {
            TOKEN_PRIMARY = 1,
            TOKEN_IMPERSONATION
        }

        public enum SecurityImpersonationLevel
        {
            SECURITY_ANONYMOUS,
            SECURITY_IDENTIFICATION,
            SECURITY_IMPERSONATION,
            SECURITY_DELEGATION
        }

        [Flags]
        public enum LogonFlags : uint
        {
            LogonWithProfile = 0x00000001,
            LogonNetCredentialsOnly = 0x00000002,
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool OpenProcessToken(
               IntPtr hProcess,
               DesiredAccess dwDesiredAccess,
               out IntPtr hToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool DuplicateTokenEx(
              IntPtr hExistingToken,
              TokenAccess dwTokenAccess,
              SECURITY_ATTRIBUTES lpTokenAttributes,
              SecurityImpersonationLevel ImpersonationLevel,
              TokenType TokenType,
              out IntPtr hNewToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool ImpersonateLoggedOnUser(
              IntPtr hToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CreateProcessWithTokenW(
            IntPtr hToken,
            LogonFlags dwLogonFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
            PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment,
            [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
        }


        //      BOOL CreateProcessWithTokenW(
        //[in]                HANDLE                hToken,
        //[in]                DWORD                 dwLogonFlags,
        //[in, optional]      LPCWSTR               lpApplicationName,
        //[in, out, optional] LPWSTR                lpCommandLine,
        //[in]                DWORD                 dwCreationFlags,
        //[in, optional]      LPVOID                lpEnvironment,
        //[in, optional]      LPCWSTR               lpCurrentDirectory,
        //[in]                LPSTARTUPINFOW        lpStartupInfo,
        //[out]               LPPROCESS_INFORMATION lpProcessInformation
        //);

        public static bool OpenProcessToken(IntPtr hProcess, DesiredAccess dwDesiredAccess, out IntPtr hToken)
        {
            object[] parameters = { hProcess, (uint)dwDesiredAccess, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"OpenProcessToken", typeof(Delegates.OpenProcessToken), ref parameters);
            hToken = (IntPtr)parameters[2];
            return retVal;
        }

        public static bool ImpersonateLoggedOnUser(IntPtr hToken)
        {
            object[] parameters = { hToken };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"ImpersonateLoggedOnUser", typeof(Delegates.ImpersonateLoggedOnUser), ref parameters);
            return retVal;
        }

        public static bool DuplicateTokenEx(
              IntPtr hExistingToken,
              TokenAccess dwTokenAccess,
              ref SECURITY_ATTRIBUTES lpTokenAttributes,
              SecurityImpersonationLevel ImpersonationLevel,
              TokenType TokenType,
              out IntPtr hNewToken)
        {
            object[] parameters = { hExistingToken, (uint)dwTokenAccess, lpTokenAttributes, (int)ImpersonationLevel, (int)TokenType, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"DuplicateTokenEx", typeof(Delegates.DuplicateTokenEx), ref parameters);
            hNewToken = (IntPtr)parameters[5];
            return retVal;
        }

        public static bool CreateProcessWithTokenW(IntPtr hToken,
            LogonFlags dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation)
        {
            object[] parameters = { hToken, (uint)dwLogonFlags, lpApplicationName, lpCommandLine, (uint)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"CreateProcessWithTokenW", typeof(Delegates.CreateProcessWithTokenW), ref parameters);
            lpProcessInformation = (PROCESS_INFORMATION)parameters[8];
            return retVal;
        }


    }


}
