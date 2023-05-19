using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;

namespace PipeServer
{

    public class Program
    {

        private static void Main(string[] args)
        {
            var pipeServer = new SimplePipeServer("Test");

            Console.WriteLine("[thread: {0}] -> Starting server.", Thread.CurrentThread.ManagedThreadId);

            pipeServer.Start();

            Thread.Sleep(30000);

            pipeServer.Stop();

            Thread.Sleep(10000);

            Console.WriteLine("\r\n[thread: {0}] -> Server closed.\r\n", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}