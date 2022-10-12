using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Threading;

namespace Commander
{
    class Program
    {


        static void Main(string[] args)
        {
            var terminal = new Terminal.Terminal();
            ServiceProvider.RegisterSingleton<ITerminal>(terminal);
            var apiCommModule = new ApiCommModule(terminal, "127.0.0.1", 5000);
            ServiceProvider.RegisterSingleton<ICommModule>(apiCommModule);
            var exec = new Executor.Executor(terminal, apiCommModule);
            ServiceProvider.RegisterSingleton<IExecutor>(exec);

            exec.Start();

            while(exec.IsRunning)
            {
                Thread.Sleep(500);
            }
        }
    }
}
