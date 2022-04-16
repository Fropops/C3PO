using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Models
{
    public class AgentTask
    {
        public string AgentId { get; set; }
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public byte[] File { get; set; }
        public DateTime RequestDate { get; set; }

        public string FullCommand
        {
            get
            {
                var full = this.Command;
                if (!string.IsNullOrEmpty(Arguments))
                    full += " " + this.Arguments;
                return full;
            }
        }

        public void Print(AgentTaskResult result)
        {
            Terminal.WriteInfo($"Task {this.Id}");
            Terminal.WriteInfo($"Cmd = {this.FullCommand}");
            if (!result.Completed)
                Terminal.WriteInfo($"Task is still running ({result.Completion}%)");
            Terminal.WriteInfo($"-------------------------------------------");
            if (!string.IsNullOrEmpty(result.Result))
                Terminal.WriteLine(result.Result);
        }
    }
}
