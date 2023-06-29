using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class AgentMetadata
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public string Hostname { get; set; }
        [FieldOrder(2)]
        public string UserName { get; set; }
        [FieldOrder(3)]
        public string ProcessName { get; set; }
        [FieldOrder(4)]
        public int ProcessId { get; set; }
        [FieldOrder(5)]
        public string Integrity { get; set; }
        [FieldOrder(6)]
        public string Architecture { get; set; }
        [FieldOrder(7)]
        public string EndPoint { get; set; }
        [FieldOrder(8)]
        public string Version { get; set; }

        public int SleepInterval { get; set; }
        public int SleepJitter { get; set; }
    }
}
