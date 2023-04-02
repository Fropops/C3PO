using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Native.Kernel32.CloseHandle(ImpersonatedToken);
            ImpersonatedToken = IntPtr.Zero;
            
        }
    }
}
