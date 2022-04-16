using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public abstract class SwitchModeCommand : ExecutorCommand
    {
        public override string Description => $"Switch to {this.TargetMode} mode";
        public override ExecutorMode AvaliableIn => ExecutorMode.None;

        public abstract ExecutorMode TargetMode { get; }
        protected override void InnerExecute(Executor executor, string parms)
        {
            executor.Mode = TargetMode;
            if (TargetMode  == ExecutorMode.None)
                executor.SetPrompt(Executor.DefaultPrompt);
            else
                executor.SetPrompt($"${this.TargetMode}> ");
        }
    }


    public class ListenerModeCommand : SwitchModeCommand
    {
        public override string Name => "listener";
        public override ExecutorMode TargetMode => ExecutorMode.Listener;

    }

    public class AgentModeCommand : SwitchModeCommand
    {
        public override string Name => "agent";
        public override ExecutorMode TargetMode => ExecutorMode.Agent;

    }
}
