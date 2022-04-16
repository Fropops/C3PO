using System;
using System.Threading;

namespace Commander
{
    class Program
    {
        public static ApiCommModule s_apiCommModule;

        static void Main(string[] args)
        {
            s_apiCommModule = new ApiCommModule("192.168.56.1", 5000);
            s_apiCommModule.Init();

            var exec = new Executor();
            exec.Init(s_apiCommModule);


            exec.Start();

            while(exec.IsRunning)
            {
                Thread.Sleep(500);
            }
        }
    }
}
