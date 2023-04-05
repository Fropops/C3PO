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

        public string Label { get; set; }
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

        public string DisplayCommand
        {
            get
            {
                var full = this.Label ?? String.Empty;
          
                if (full.Length > 30)
                    return full.Substring(0, 30) + "...";

                return full;
            }
        }

        public void Print(AgentTaskResult result, ITerminal terminal)
        {
            //terminal.WriteInfo($"Task {this.Id}");
            //terminal.WriteInfo($"Label = {this.Label}");
            //terminal.WriteInfo($"Cmd = {this.FullCommand}");
            //if(result.Status == AgentResultStatus.Completed)
            //    terminal.WriteInfo($"Task is {result.Status} ");
            //else
            //    if(result.Status == AgentResultStatus.Running && !string.IsNullOrEmpty(result.Info))
            //    terminal.WriteLine($"Task is {result.Status} : {result.Info}");
            //else
            //    terminal.WriteLine($"Task is {result.Status} ");
            //terminal.WriteInfo($"-------------------------------------------");
            //if (!string.IsNullOrEmpty(result.Result))
            //    terminal.WriteLine(result.Result);

            var cmd = this.DisplayCommand;
            var status = result.Status;

            terminal.WriteInfo($"Task {cmd} is {status}");
            if (result.Status == AgentResultStatus.Running && !string.IsNullOrEmpty(result.Info))
                terminal.WriteLine($"Task is {result.Status} : {result.Info}");
            else
            if (!string.IsNullOrEmpty(result.Result))
            {
                terminal.WriteInfo($"-------------------------------------------");
                terminal.WriteLine(result.Result);
            }
        }
    }
}
