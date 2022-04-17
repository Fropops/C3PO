using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public abstract class ExecutorCommand
    {
        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public abstract ExecutorMode AvaliableIn { get; }

        public virtual void Execute(string parms)
        {
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();
            InnerExecute(terminal, executor, comm, parms);
            executor.InputHandled(this, true);
        }

        protected abstract void InnerExecute(ITerminal terminal, IExecutor executor, ICommModule comm, string parms);
    }
}
