using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocksProxy
{
    class Program
    {
        static void Main(string[] args)
        {


            var proxy = new Socks4Proxy();
            proxy.Start();
            //while (true)
            //{

            //}

            Thread.Sleep(5000);
            proxy.Stop();

            Thread.Sleep(5000);

        }
    }
}
