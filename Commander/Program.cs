using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace Commander
{
    class Program
    {


        static void Main(string[] args)
        {
            IConfiguration Config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build();

            var c = new CommanderConfig();
            var sec = Config.GetSection("Api");
            c.Address = sec.GetValue<string>("Address");
            c.Port = sec.GetValue<int>("Port");
            c.User = sec.GetValue<string>("User");
            c.ApiKey = sec.GetValue<string>("ApiKey");



            var terminal = new Terminal.Terminal();
            ServiceProvider.RegisterSingleton<ITerminal>(terminal);
            var apiCommModule = new ApiCommModule(terminal, c);
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
