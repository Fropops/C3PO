﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class AgentMetadataResponse
    {
        public string Id { get; set; }

        public string Hostname { get; set; }
        public string UserName { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Architecture { get; set; }

        public string EndPoint { get; set; }
        public string Version { get; set; }
        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }

    }

    public class AgentResponse
    {
        public AgentMetadataResponse Metadata { get; set; }

        public DateTime LastSeen { get; set; }

        public DateTime FirstSeen { get; set; }

        public string ListenerId { get; set; }
        public List<string> Path { get; set; }
    }
}
