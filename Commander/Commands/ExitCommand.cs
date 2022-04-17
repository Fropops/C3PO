using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class ExitCommand : ExecutorCommand
    {
        public override string Description => "Close the Commander";
        public override string Name => "exit";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(ITerminal terminal, IExecutor executor, ICommModule comm, string parms)
        {
            executor.Stop();
        }
    }
}
