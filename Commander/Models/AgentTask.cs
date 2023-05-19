using Commander.Commands.Agent;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
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

        public void Print(AgentTaskResult result, ITerminal terminal, bool fullLabel = false)
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

            var cmd = fullLabel ? this.DisplayCommand : this.Label;
            var status = result.Status;


            if (result.Status == AgentResultStatus.Running && !string.IsNullOrEmpty(result.Info))
            {
                terminal.WriteLine($"Task is {result.Status} : {result.Info}");
                return;
            }

            terminal.WriteInfo($"Task {cmd} is {status}");

            if (result.Status == AgentResultStatus.Completed)
            {
                terminal.WriteInfo($"-------------------------------------------");
                if (!string.IsNullOrEmpty(result.Result))
                    terminal.WriteLine(result.Result);
                this.WriteObjects(result, terminal);

                if (!string.IsNullOrEmpty(result.Error))
                    terminal.WriteError(result.Error);
            }
            return;
        }

        private void WriteObjects(AgentTaskResult result, ITerminal terminal)
        {
            if (string.IsNullOrEmpty(result.Objects))
                return;


            if (this.Command == EndPointCommand.LS)
            {
                var json = result.ObjectsAsJson;
                var list = JsonConvert.DeserializeObject<List<LSResult>>(json);
                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Name").LeftAligned());
                table.AddColumn(new TableColumn("Type").LeftAligned());
                table.AddColumn(new TableColumn("Length").LeftAligned());
                foreach (var item in list)
                {
                    long lengthInBytes = item.Length;
                    double lengthInKb = lengthInBytes / 1024.0;
                    double lengthInMb = lengthInKb / 1024.0;
                    double lengthInGb = lengthInMb / 1024.0;

                    string lengthString = lengthInBytes < 1024
                    ? $"{item.Length} bytes"
                    : lengthInKb < 1024
                        ? $"{lengthInKb:F1} KB"
                        : lengthInMb < 1024
                            ? $"{lengthInMb:F1} MB"
                            : $"{lengthInGb:F1} GB";
                    table.AddRow(item.Name, item.IsFile ? "File" : "Dirrectory", lengthString);
                }

                terminal.Write(table);
                return;
            }

            if (this.Command == EndPointCommand.PS)
            {
                var json = result.ObjectsAsJson;
                var list = JsonConvert.DeserializeObject<List<PSResult>>(json);

                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Name").LeftAligned());
                table.AddColumn(new TableColumn("Id").LeftAligned());
                table.AddColumn(new TableColumn("ParentId").LeftAligned());
                table.AddColumn(new TableColumn("Owner").LeftAligned());
                table.AddColumn(new TableColumn("Arch.").LeftAligned());
                table.AddColumn(new TableColumn("Session").LeftAligned());
                table.AddColumn(new TableColumn("Path").LeftAligned());

                this.RenderPSTree(list, table);

                terminal.Write(table);
                return;
            }
        }

        private void RenderPSTree(List<PSResult> nodes, Table table)
        {

            var rootsNodes = new List<PSResult>();
            foreach (var node in nodes)
            {
                if (node.Name == "brave")
                {
                    int i = 0;
                }
                if (node.Id == 0)
                    continue;
                if (node.ParentId == 0)
                    rootsNodes.Add(node);
                if (!nodes.Any(p => p.Id == node.ParentId))
                    rootsNodes.Add(node);
            }

            foreach (var child in rootsNodes.OrderBy(n => n.Name))
                RenderNode(nodes, child, table, 0);

        }

        private void RenderNode(List<PSResult> nodes, PSResult node, Table table, int indent)
        {
            table.AddRow(
                SurroundIfSelf(node, node.Name.PadLeft(indent + node.Name.Length)),
                SurroundIfSelf(node, node.Id.ToString()),
                SurroundIfSelf(node, node.ParentId.ToString()),
                SurroundIfSelf(node, node.Owner),
                SurroundIfSelf(node, node.Arch),
                SurroundIfSelf(node, node.SessionId.ToString()),
                SurroundIfSelf(node, node.ProcessPath));
            foreach (var child in nodes.Where(p => p.ParentId == node.Id).OrderBy(n => n.Name))
                RenderNode(nodes, child, table, indent + 3);
        }

        private IRenderable SurroundIfSelf(PSResult res, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new Markup(string.Empty);

            var exec = ServiceProvider.GetService<IExecutor>();
            if (exec.CurrentAgent != null && exec.CurrentAgent.Metadata.ProcessId == res.Id)
                return new Markup($"[cyan]{value}[/]");

            return new Markup(value);
        }


        public class LSResult
        {
            public long Length { get; set; }
            public string Name { get; set; }
            public bool IsFile { get; set; }
        }

        public class PSResult
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public int ParentId { get; set; }
            public int SessionId { get; set; }
            public string ProcessPath { get; set; }
            public string Owner { get; set; }
            public string Arch { get; set; }
        }
    }
}
