using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.DInvoke;

namespace Agent.Commands
{

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
    public class ListProcessCommand : AgentCommand
    {
        public override string Name => "ps";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var list = new List<PSResult>();
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
                var res = new PSResult()
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    ParentId = GetParentId(process.Id),
                    SessionId = process.SessionId,
                    ProcessPath = GetProcessPath(process),
                    Owner = GetProcessOwner(process),
                    Arch = GetProcessArch(process),
                };

                list.Add(res);
            }

            context.Objects(list);
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
                if (!Advapi.OpenProcessToken(proc.Handle, DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
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
                Kernel32.CloseHandle(hToken);
            }
        }

        public static bool Is64bitProcess(Process proc)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return false;


                if (!WinAPI.PInvoke.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
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


                if (!WinAPI.PInvoke.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
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

        private int GetParentId(int processId)
        {
            try
            {
                var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", processId);
                var search = new ManagementObjectSearcher("root\\CIMV2", query);
                var results = search.Get().GetEnumerator();
                results.MoveNext();
                var queryObj = results.Current;
                var parentId = (uint)queryObj["ParentProcessId"];
                return (int)parentId;
            }
            catch
            {
                return 0;
            }
        }



    }
}
