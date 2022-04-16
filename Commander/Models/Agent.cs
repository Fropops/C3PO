using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Models
{
    public class Agent
    {
        public AgentMetadata Metadata { get; set;  }

        public DateTime LastSeen { get; set; }
    }
}
