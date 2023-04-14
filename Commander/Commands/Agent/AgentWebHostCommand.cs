using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Commander.Commands.Agent
{
    public class AgentWebHostCommandOptions
    {
        public string verb { get; set; }
        public string path { get; set; }
        public string file { get; set; }
        public bool powershell { get; set; }
        public string description { get; set; }
    }

    public class AgentWebHostCommand : EnhancedCommand<AgentWebHostCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "WebHost on the agent";
        public override string Name => "host";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
               new Argument<string>("verb").FromAmong(WebHostVerbs.Push, WebHostVerbs.Remove, WebHostVerbs.Show, WebHostVerbs.Script, WebHostVerbs.Log, WebHostVerbs.Clear),

                new Option<string>(new[] { "--file", "-f" }, () => null, "Path of the local file to push (" + WebHostVerbs.Push + ")"),
                new Option<string>(new[] { "--path", "-p" }, () => null, "Hosting path (" + WebHostVerbs.Push + "," + WebHostVerbs.Show + ")"),
                new Option<bool>(new[] { "--powershell", "-ps" }, () => false, "Specify is the file is a powershell script (" + WebHostVerbs.Push + ")"),
                new Option<string>(new[] { "--description", "-d" }, () => null, "Description of the file (" + WebHostVerbs.Push + ")"),
            };

        protected async override Task<bool> HandleCommand(CommandContext<AgentWebHostCommandOptions> context)
        {
            if (!this.Validate(context))
            {
                return false;
            }


            var agent = context.Executor.CurrentAgent;
            string commandArgs = null;

            if (context.Options.verb == WebHostVerbs.Push)
            {
                byte[] fileBytes = File.ReadAllBytes(context.Options.file);

                await PushFile(context, context.Executor.CurrentAgent, context.Options.path, fileBytes, context.Options.powershell, context.Options.description);

                bool first = true;
                var fileId = await context.CommModule.Upload(fileBytes, Path.GetFileName(context.Options.path), a =>
                {
                    context.Terminal.ShowProgress("uploading", a, first);
                    first = false;
                });

                context.Terminal.WriteSuccess($"File {context.Options.file} tasked to be hosted on agent at {context.Options.path}.");
                return true;
            }
            else
            {
                commandArgs = $"{context.Options.verb}";
                if (!string.IsNullOrEmpty(context.Options.path))
                {
                    commandArgs += $" {context.Options.path}";
                }
            }

            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, commandArgs).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");

            return true;
        }

        public bool Validate(CommandContext<AgentWebHostCommandOptions> context)
        {
            if (context.Options.verb == WebHostVerbs.Push)
            {
                if (string.IsNullOrEmpty(context.Options.file))
                {
                    context.Terminal.WriteError($"[X] File is mandatory");
                    return false;
                }

                if (string.IsNullOrEmpty(context.Options.path))
                {
                    context.Terminal.WriteError($"[X] Path is mandatory");
                    return false;
                }

                if (!File.Exists(context.Options.file))
                {
                    context.Terminal.WriteError($"[X] File {context.Options.file} not found");
                    return false;
                }
            }

            if (context.Options.verb == WebHostVerbs.Remove)
            {
                if (string.IsNullOrEmpty(context.Options.path))
                {
                    context.Terminal.WriteError($"[X] Path is mandatory");
                    return false;
                }
            }

            return true;
        }

        //private async Task Stage(CommandContext<WebHostCommandOptions> context)
        //{
        //    var files = new List<string> { "Agent.exe", "Agent-x86.exe", "Stage1.dll", "Stage1-x86.dll" };
        //    foreach(var f in files)
        //    {
        //        var fileBytes = GenerateB64(f);
        //        var fileName = Path.GetFileNameWithoutExtension(f) + ".b64";
        //        bool first = true;
        //        var fileId = await context.CommModule.Upload(fileBytes, fileName, a =>
        //        {
        //            context.Terminal.ShowProgress("uploading", a, first);
        //            first = false;
        //        });
        //        await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName, "push");
        //        //context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
        //    }


        //}

        //public byte[] GenerateB64(string sourceFile)
        //{
        //    var inputFile = Path.Combine(BuildHelper.SourceFolder, sourceFile);
        //    byte[] bytes = File.ReadAllBytes(inputFile);
        //    string base64 = Convert.ToBase64String(bytes);
        //    return Encoding.UTF8.GetBytes(base64);
        //}

        public static async Task PushFile(CommandContext context, Models.Agent agent, string path, byte[] fileBytes, bool isPowershell = false, string description = null)
        {
            string prm = $"push {isPowershell}";
            if (!string.IsNullOrEmpty(description))
                prm += " \"" + description + "\"";

            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, path, a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });
            await context.CommModule.TaskAgent($"host {path}", Guid.NewGuid().ToString(), agent.Metadata.Id, "host", fileId, path, prm);
        }
    }


}
