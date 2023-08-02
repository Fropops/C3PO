using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;

namespace Commander.Commands.Agent.Service
{
    public class JobCommandOptions : ServiceCommandOptions
    {
        public int? id { get; set; }
    }
    public class JobCommand : ServiceCommand<JobCommandOptions>
    {
        public override string Description => "Manage Jobs";
        public override string Name => "job";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.Job;
        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("verb", () => "show", "show | kill").FromAmong("show", "kill"),
            new Option<int?>(new[] { "--id", "-i" }, () => null, "Id of the job (Kill)"),
        };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(ServiceVerb.Kill, this.Kill);
            //this.Register(ServiceVerb.Show, this.Show);
        }

        protected async Task<bool> Kill(CommandContext<JobCommandOptions> context)
        {
            if(!context.Options.id.HasValue)
            {
                context.Terminal.WriteError($"Id is mandatory for kill verb");
                return false;
            }

            context.AddParameter(ParameterId.Id, context.Options.id.Value);

            return true;
        }


    }
}
