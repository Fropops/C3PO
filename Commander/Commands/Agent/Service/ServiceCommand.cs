using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;

namespace Commander.Commands.Agent.Service
{
    public class ServiceCommandOptions
    {
        public string verb { get; set; }
    }
    public abstract class ServiceCommand<T> : EndPointCommand<T> where T : ServiceCommandOptions
    {
        protected Dictionary<ServiceVerb, Func<CommandContext<T>, Task<bool>>> dico = new Dictionary<ServiceVerb, Func<CommandContext<T>, Task<bool>>>();
        public override string Category => CommandCategory.Services;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.Job;

        public ServiceCommand()
        {
            this.RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
        }

        public void Register(ServiceVerb verb, Func<CommandContext<T>, Task<bool>> action)
        {
            dico.Add(verb, action);
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            if (!await this.CheckParams(context))
                return false;
            
            var verb = (ServiceVerb)Enum.Parse(typeof(ServiceVerb), context.Options.verb, true);
            context.AddParameter(ParameterId.Verb, verb);

            if (dico.TryGetValue(verb, out var action))
                if(!await action(context))
                    return false;

            this.SpecifyParameters(context);

            await this.CallEndPointCommand(context);
            return true;
        }

    }
}
