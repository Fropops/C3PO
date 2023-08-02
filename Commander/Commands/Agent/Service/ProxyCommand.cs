﻿using Commander.Commands.Agent.Service;
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
    public class ProxyCommandOptions : ServiceCommandOptions
    {
        public int? port { get; set; }
    }

    public class ProxyCommand : ServiceCommand<ProxyCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Start a Socks4 Proxy on the agent";
        public override string Name => "proxy";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.Proxy;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", "start | stop | show").FromAmong("start", "stop", "show"),
                new Option<int?>(new[] { "--port", "-p" }, () => 1080, "port to use on the server"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(ServiceVerb.Start, this.Start);
            this.Register(ServiceVerb.Stop, this.Stop);
            this.Register(ServiceVerb.Show, this.Show);
        }

        protected async Task<bool> Start(CommandContext<ProxyCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (!context.Options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to start the proxy!");
                return false;
            }
            var res = await context.CommModule.StartProxy(agent.Metadata.Id, context.Options.port.Value);
            if (!res)
            {
                context.Terminal.WriteError("[X] Cannot start proxy on the server!");
                return false;
            }

            context.Terminal.WriteSuccess("[*] Proxy server started !");
            return true;
        }

        protected async Task<bool> Stop(CommandContext<ProxyCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            var res = await context.CommModule.StopProxy(agent.Metadata.Id);
            if (!res)
            {
                context.Terminal.WriteError("[X] Cannot stop proxy on the server!");
            }
            context.Terminal.WriteSuccess("[*] Proxy server stopped !");
            return true;
        }

        protected async Task<bool> Show(CommandContext<ProxyCommandOptions> context)
        {
            var res = await context.CommModule.ShowProxy();
            if (!res.Any())
            {
                context.Terminal.WriteLine("[>] No proxy running!");
                return true;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Agent").LeftAligned());
            table.AddColumn(new TableColumn("Port").LeftAligned());
            foreach (var item in res)
            {
                table.AddRow(item.AgentId, item.Port.ToString());
            }

            context.Terminal.Write(table);

            return true;
        }

        protected override async Task CallEndPointCommand(CommandContext<ProxyCommandOptions> context)
        {
            //override to not send task to Agent
        }


    }
}
