using System;
using WinAPI.DInvoke;

namespace Agent.Helpers
{
    public static class ImpersonationHelper
    {
        public static IntPtr ImpersonatedToken { get; private set; } = IntPtr.Zero;
        public static bool HasCurrentImpersonation
        {
            get
            {
                //Console.WriteLine($"Impersonated Token = {ImpersonatedToken}");
                if (ImpersonatedToken == IntPtr.Zero)
                    return false;

                return true;
            }
        }

        public static void Impersonate(IntPtr token)
        {
            if (HasCurrentImpersonation)
                Reset();
            ImpersonatedToken = token;
        }

        public static void Reset()
        {
            if (!HasCurrentImpersonation)
                return;
            Kernel32.CloseHandle(ImpersonatedToken);
            ImpersonatedToken = IntPtr.Zero;

        }
    }
}
