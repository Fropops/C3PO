using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UACBypass
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                UacBypass.verbose = false;
                var module = new UacBypass();
                module.Execute(args[0]);
            }
            catch (Exception exc)
            {
                UacBypass.Log(exc.ToString());
            }

            Environment.Exit(-1);
        }
    }
}
