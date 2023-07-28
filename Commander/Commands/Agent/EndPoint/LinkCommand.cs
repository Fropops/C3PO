using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;
using Commander.Commands.Agent.Service;
using Common;

namespace Commander.Commands.Agent.EndPoint
{
    public class LinkCommandOptions : ServiceCommandOptions
    {
        public string bindto { get; set; }
    }
    public class LinkCommand : ServiceCommand<LinkCommandOptions>
    {
        public override string Description => "Link to another Agent";
        public override string Name => "link";

        public override CommandId CommandId => CommandId.Link;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", () => "show", "show | start | stop").FromAmong("show", "start", "stop"),
                 new Option<string>(new[] { "--bindto", "-b" }, () => null, "Endpoint To bind to"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(ServiceVerb.Start, this.Start);
            this.Register(ServiceVerb.Stop, this.Stop);
            //this.Register(ServiceVerb.Show, this.Show);
        }

        protected bool Start(CommandContext<LinkCommandOptions> context)
        {
            var url = context.Options.bindto;

            ConnexionUrl conn = ConnexionUrl.FromString(url);
            if (conn == null || !conn.IsValid)
            {
                context.Terminal.WriteError($"BindTo is not valid!");
                return false;
            }

            context.AddParameter(ParameterId.Bind, conn.ToString());

            return true;
        }

        protected bool Stop(CommandContext<LinkCommandOptions> context)
        {
            var url = context.Options.bindto;

            ConnexionUrl conn = ConnexionUrl.FromString(url);
            if (conn == null || !conn.IsValid)
            {
                context.Terminal.WriteError($"BindTo is not valid!");
                return false;
            }

            context.AddParameter(ParameterId.Bind, conn.ToString());

            return true;
        }

    }
}
