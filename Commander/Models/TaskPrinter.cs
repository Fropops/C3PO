﻿using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using Commander.Executor;
using Commander.Terminal;
using Common.Models;
using Shared;
using Shared.ResultObjects;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Commander.Models
{
    public class TaskPrinter
    {
        public static void Print(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal, bool fullLabel = false)
        {
            var cmd = task.Command;
            var status = result.Status;


            if (result.Status == AgentResultStatus.Running)
            {
                var txt = $"Task is {result.Status}";
                if (!string.IsNullOrEmpty(result.Info))
                    txt += $" : { result.Info}";
                terminal.WriteInfo(txt);
                terminal.WriteInfo($"-------------------------------------------");
            }

            if (result.Status == AgentResultStatus.Completed || result.Status == AgentResultStatus.Error)
            {
                terminal.WriteInfo($"Task {cmd} is {status}");
                terminal.WriteInfo($"-------------------------------------------");
            }

            if (result.Status == AgentResultStatus.Queued)
            {
                terminal.WriteInfo($"Task {cmd} is {status}");
                return;
            }


            if (!string.IsNullOrEmpty(result.Output))
                terminal.WriteLine(result.Output);

            if (!string.IsNullOrEmpty(result.Error))
                terminal.WriteError(result.Error);

            WriteObjects(task, result, terminal);
            return;
        }

        private static void WriteObjects(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal)
        {
            if (result.Objects == null || result.Objects.Length == 0)
                return;

            switch (task.CommandId)
            {
                case CommandId.Ls:
                    PrintLs(task, result, terminal);
                    break;

                case CommandId.Job:
                    PrintJobs(task, result, terminal);
                    break;

                case CommandId.Link:
                    PrintLinks(task, result, terminal);
                    break;
                case CommandId.ListProcess:
                    PrintProcessList(task, result, terminal);
                    break;


                default: break;
            }

            terminal.WriteLine();
        }

        private static void PrintLs(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal)
        {
            var list = result.Objects.BinaryDeserializeAsync<List<ListDirectoryResult>>().Result;
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
                table.AddRow(item.Name, item.IsFile ? "File" : "Dir", lengthString);
            }

            terminal.Write(table);
        }

        private static void PrintJobs(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal)
        {
            var list = result.Objects.BinaryDeserializeAsync<List<Job>>().Result;
            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Id").LeftAligned());
            table.AddColumn(new TableColumn("Name").LeftAligned());
            table.AddColumn(new TableColumn("Type").LeftAligned());
            table.AddColumn(new TableColumn("ProcessId").LeftAligned());
            foreach (var item in list)
                table.AddRow(item.Id.ToString(), item.Name, item.JobType.ToString(), item.ProcessId.ToString());

            terminal.Write(table);
        }

        private static void PrintLinks(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal)
        {
            var list = result.Objects.BinaryDeserializeAsync<List<LinkInfo>>().Result;
            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Agent Id").LeftAligned());
            table.AddColumn(new TableColumn("Binding").LeftAligned());

            foreach (var item in list)
                table.AddRow(item.ChildId.ToString(), item.Binding?.ToString());

            terminal.Write(table);
        }

        private static void PrintProcessList(TeamServerAgentTask task, AgentTaskResult result, ITerminal terminal)
        {
            var list = result.Objects.BinaryDeserializeAsync<List<ListProcessResult>>().Result;
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

            RenderPSTree(list, table);

            terminal.Write(table);
        }


        private static void RenderPSTree(List<ListProcessResult> nodes, Table table)
        {

            var rootsNodes = new List<ListProcessResult>();
            foreach (var node in nodes)
            {
                //if (node.Name == "brave")
                //{
                //    int i = 0;
                //}
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

        private static void RenderNode(List<ListProcessResult> nodes, ListProcessResult node, Table table, int indent)
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

        private static IRenderable SurroundIfSelf(ListProcessResult res, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new Markup(string.Empty);

            var exec = ServiceProvider.GetService<IExecutor>();
            if (exec.CurrentAgent != null && exec.CurrentAgent.Metadata.ProcessId == res.Id)
                return new Markup($"[cyan]{value}[/]");

            return new Markup(value);
        }
    }
}
