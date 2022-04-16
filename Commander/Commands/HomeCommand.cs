using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class HomeCommand : ExecutorCommand
    {
        public override string Description => "Return to home mode";
        public override string Name => "home";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(Executor executor, string parms)
        {
            executor.Mode = ExecutorMode.None;
            executor.SetPrompt(Executor.DefaultPrompt);
        }
    }
}
