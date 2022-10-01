using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class CreateListenersCommandOptions
    {
        public string name { get; set; }
        public bool verbose { get; set; }
        public int? localPort { get; set; }

        public string address { get; set; }
        public bool secured { get; set; }
        public int? publicPort {get;set;}

    }


    public class InteractAgentCommand : EnhancedCommand<CreateListenersCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Start a new listener";
        public override string Name => "start";
        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("name", "name of the listener"),
                new Argument<string>("address", "The listening address."),
                new Option<int?>(new[] { "--localPort", "-p" }, () => null, "The listening port."),
                new Option<int?>(new[] { "--publicPort", "-pp" }, () => null, "The public listening port."),
                new Option<bool>(new[] { "--secured", "-s" }, () => true, "HTTPS if secured else HTTP"),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(CommandContext<CreateListenersCommandOptions> context)
        {
            if(!context.Options.localPort.HasValue && context.Options.publicPort.HasValue)
            {
                context.Options.localPort = context.Options.publicPort;
            }

            if (!context.Options.publicPort.HasValue && context.Options.localPort.HasValue)
            {
                context.Options.publicPort = context.Options.localPort;
            }

            if(!context.Options.publicPort.HasValue)
            {
                if (context.Options.secured)
                    context.Options.publicPort = 443;
                else
                    context.Options.publicPort = 80;
            }

            if (!context.Options.localPort.HasValue)
            {
                if (context.Options.secured)
                    context.Options.localPort = 443;
                else
                    context.Options.localPort = 80;
            }


            if (context.CommModule.GetListeners().Any(l => l.Name.ToLower().Equals(context.Options.name.ToLower())))
            {
                context.Terminal.WriteError($"A listener whith the name {context.Options.name} already exists !");
                return false;
            }

            var result = await context.CommModule.CreateListener(context.Options.name, context.Options.localPort.Value, context.Options.address, context.Options.secured, context.Options.publicPort.Value);
            if (!result.IsSuccessStatusCode)
            {
                context.Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var listener = JsonConvert.DeserializeObject<ListenerResponse>(json);

            context.Terminal.WriteSuccess($"Listener {listener.Name} started on port {listener.BindPort}.");
            return true;
        }
    }

}
