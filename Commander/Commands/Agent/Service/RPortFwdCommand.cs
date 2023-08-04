using Commander.Commands.Agent.Service;
using Commander.Communication;
using Commander.Executor;
using Commander.Models;
using Commander.Terminal;
using Shared;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class RPortFwdCommandOptions : ServiceCommandOptions
    {
        public int? port { get; set; }
        public string destHost { get; set; }
        public int? destPort { get; set; }
    }

    public class RPortFwdCommand : ServiceCommand<RPortFwdCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Start a Reverse Port Forward on the agent";
        public override string Name => "rportfwd";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.RportFwd;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", () => "show", "start | stop | show").FromAmong("start", "stop", "show"),
                new Option<int?>(new[] { "--port", "-p" }, () => null, "port to use on the agent"),
                new Option<string>(new[] { "--destHost", "-h" }, () => null, "host to use as destination"),
                new Option<int?>(new[] { "--destPort", "-d" }, () => null, "port to use as destination"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(ServiceVerb.Start, this.Start);
            this.Register(ServiceVerb.Stop, this.Stop);
        }

        protected async Task<bool> Start(CommandContext<RPortFwdCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (!context.Options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to start the port forward!");
                return false;
            }
            if (string.IsNullOrEmpty(context.Options.destHost))
            {
                context.Terminal.WriteError("[X] Destination Host is required to start the port forward!");
                return false;
            }
            if (!context.Options.destPort.HasValue)
            {
                context.Terminal.WriteError("[X] Destination Port is required to start the port forward!");
                return false;
            }

            ReversePortForwardDestination dest = new ReversePortForwardDestination()
            {
                Hostname = context.Options.destHost,
                Port = context.Options.destPort.Value
            };

            context.AddParameter(ParameterId.Port, context.Options.port.Value);
            context.AddParameter(ParameterId.Parameters, dest);

            return true;
        }

        protected async Task<bool> Stop(CommandContext<RPortFwdCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (!context.Options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to stop the port forward!");
                return false;
            }

            context.AddParameter(ParameterId.Port, context.Options.port.Value);

            return true;
        }
    }
}
