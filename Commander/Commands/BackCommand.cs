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
    public class BackCommand : ExecutorCommand
    {
        public override string Description => "Return to parent mode";
        public override string Name => "back";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(ITerminal terminal, IExecutor executor, ICommModule comm, string parms)
        {
            switch(executor.Mode)
            {
                case ExecutorMode.AgentInteraction:
                    {
                        executor.CurrentAgent = null;
                        executor.Mode = ExecutorMode.Agent;
                    }break;
                default:
                    {
                        executor.Mode = ExecutorMode.None;
                    }break;
            }

            if (executor.Mode  == ExecutorMode.None)
                terminal.Prompt = Terminal.Terminal.DefaultPrompt;
            else
                terminal.Prompt = $"${executor.Mode}> ";

        }
    }
}
