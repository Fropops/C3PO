using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands.Services
{
    public abstract class ServiceCommand<T> : AgentCommand
    {
        protected T Service;
        protected Dictionary<ServiceVerb, Action<AgentTask, AgentCommandContext>> dico = new Dictionary<ServiceVerb, Action<AgentTask, AgentCommandContext>>();
        public ServiceCommand()
        {
            Service = ServiceProvider.GetService<T>();
            this.RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
            this.Register(ServiceVerb.Show, this.Show);
        }

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var verb = task.GetParameter<ServiceVerb>(ParameterId.Verb);
            if (dico.TryGetValue(verb, out var action))
                action(task, context);
            else
                context.Error($"Verb {verb} not found !");
        }

        public void Register(ServiceVerb verb, Action<AgentTask, AgentCommandContext> action)
        {
            dico.Add(verb, action);
        }

        protected abstract void Show(AgentTask task, AgentCommandContext context);
    }
}
