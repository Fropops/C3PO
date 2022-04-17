using Commander.Terminal;
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

        public void Print(AgentTaskResult result, ITerminal terminal)
        {
            terminal.WriteInfo($"Task {this.Id}");
            terminal.WriteInfo($"Cmd = {this.FullCommand}");
            if (!result.Completed)
                terminal.WriteInfo($"Task is still running ({result.Completion}%)");
            terminal.WriteInfo($"-------------------------------------------");
            if (!string.IsNullOrEmpty(result.Result))
                terminal.WriteLine(result.Result);
        }
    }
}
