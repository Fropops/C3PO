using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Models
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
        public string FileId { get; set; }
        public string FileName { get; set; }
        public AgentResultStatus Status { get; set; }
    }
}
