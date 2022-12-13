using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class ResultData
    {
        public AgentMetadata Metadata { get; set; }
        public AgentTaskResult[] Results { get; set; }
    }
}
