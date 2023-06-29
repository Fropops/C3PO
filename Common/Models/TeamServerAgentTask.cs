using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class TeamServerAgentTask
    {
        public string Id { get; set; }

        public string AgentId { get; set; }

        public string Command { get;set; }

        public DateTime RequestDate { get; set; }

        public TeamServerAgentTask()
        {

        }
        public TeamServerAgentTask(string id, string agentId, string command, DateTime requestDate)
        {
            this.Id = id;
            this.AgentId = agentId;
            this.Command = command;
            this.RequestDate = requestDate;
        }
    }
}
