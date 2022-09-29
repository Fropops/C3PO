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
    public class ExecuteAssemblyModuleCommandOptions
    {

        public string parameters { get; set; }
    }
    public abstract class ExecuteAssemblyModuleCommand : EnhancedCommand<ExecuteAssemblyModuleCommandOptions>
    {
        public override string Category => CommandCategory.Module;
        public override string Description => "Execute a dot net assembly in memory";

        public virtual string ExeName { get; set; }

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("parameters", () => "", "parameters to pass"),
            };

        protected override async Task<bool> HandleCommand(CommandContext<ExecuteAssemblyModuleCommandOptions> context)
        {
            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(Path.Combine(InjectCommand.ModuleFolder, this.ExeName)))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            string fileName = this.ExeName;
            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, Path.GetFileName(fileName), a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });

            File.Delete(fileName);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, EndPointCommand.EXECUTEASSEMBLY, fileId, fileName, context.Options.parameters);
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.ShortId}.");
            return true;
        }
    }
}

