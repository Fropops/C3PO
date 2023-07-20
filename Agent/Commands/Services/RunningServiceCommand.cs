using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands.Services
{
    public abstract class RunningServiceCommand<T> : ServiceCommand<T> where T : IRunningService
    {
        public RunningServiceCommand() : base()
        {
            Service = ServiceProvider.GetService<T>();
            this.Register(ServiceVerb.Start, this.Start);
            this.Register(ServiceVerb.Stop, this.Stop);
        }
       
        protected abstract void Start(AgentTask task, AgentCommandContext context);

        protected abstract void Stop(AgentTask task, AgentCommandContext context);


    }
}
