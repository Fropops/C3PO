using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands.Services
{
    public abstract class ServiceCommand<T> : AgentCommand where T : IRunningService
    {
        protected T Service;
        protected Dictionary<string, Action<AgentTask, AgentCommandContext, string[]>> dico = new Dictionary<string, Action<AgentTask, AgentCommandContext, string[]>>();
        public ServiceCommand()
        {
            Service = ServiceProvider.GetService<T>();
            this.Register(ServiceVerbs.StartVerb, this.Start);
            this.Register(ServiceVerbs.StopVerb, this.Stop);
            this.Register(ServiceVerbs.ShowVerb, this.Show);
        }
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var verb = task.SplittedArgs[0];
            int argLength = task.SplittedArgs.Length - 1;
            var args = new string[argLength];
            for (int i = 0; i < argLength; ++i)
                args[i] = task.SplittedArgs[i+1];
            if (dico.TryGetValue(verb, out var action))
                action(task, context, args);
        }

        public void Register(string verb, Action<AgentTask, AgentCommandContext, string[]> action)
        {
            dico.Add(verb, action);
        }

        protected abstract void Start(AgentTask task, AgentCommandContext context, string[] args);

        protected abstract void Stop(AgentTask task, AgentCommandContext context, string[] args);

        protected virtual void Show(AgentTask task, AgentCommandContext context, string[] args)
        {

        }
    }
}
