using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public enum AgentResultStatus
    {
        Queued = 0,
        Running = 1,
        Completed = 2
    }

    public class AgentTaskResult
    {
        public string Id { get; set; }
        public string Result { get; set; }
        public string Info { get; set; }
        public AgentResultStatus Status { get; set; }
    }
}
