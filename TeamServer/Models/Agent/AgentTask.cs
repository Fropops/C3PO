using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class AgentTask
    {
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public byte[] File { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
