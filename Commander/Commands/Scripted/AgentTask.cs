using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Composite
{
    public class AgentTask
    {
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
    }
}
