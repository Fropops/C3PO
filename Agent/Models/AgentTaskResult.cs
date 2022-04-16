using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class AgentTaskResult
    {
        public string Id { get; set; }
        public string Result { get; set; }

        public int Completion { get; set; }
        public bool Completed { get; set; }
    }
}
