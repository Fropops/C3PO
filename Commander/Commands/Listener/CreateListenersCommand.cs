using ApiModels.Response;
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
        public int? port { get; set; }

    }


    public class InteractAgentCommand : EnhancedCommand<CreateListenersCommandOptions>
    {
        public override string Description => "Start a new listener";
        public override string Name => "start";
        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("name", "name of the listener"),
                new Option<int?>(new[] { "--port", "-p" }, "The listening port."),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(CreateListenersCommandOptions options)
        {
            var result = await this.Executor.CommModule.CreateListener(options.name, options.port ?? 80);
            if (!result.IsSuccessStatusCode)
            {
               Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var listener = JsonConvert.DeserializeObject<ListenerResponse>(json);

            Terminal.WriteSuccess($"Listener {listener.Name} started on port {listener.BindPort}.");
            return true;
        }
    }

}
