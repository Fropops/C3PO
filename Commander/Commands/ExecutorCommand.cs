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

        public virtual void Execute(Executor executor, string parms)
        {
            InnerExecute(executor, parms);
            executor.InputHandled(this, true);
        }

        protected abstract void InnerExecute(Executor executor, string parms);
    }
}
