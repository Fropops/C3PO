using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Response
{
    public class AgentTaskResponse
    {
        public string AgentId { get; set; }

        public string Label { get; set; }
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string[] FileNames { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
