using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using WinAPI.Wrapper;
using WinAPI;
using static Pinvoke.Kernel32;
using Pinvoke;
using System.Threading;

namespace Win32Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //@"c:\windows\system32\whoami.exe"

            try
            {
                //SpawnProcess.Run(args);
                SpawnInject.Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Oooops something went wrong....");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
