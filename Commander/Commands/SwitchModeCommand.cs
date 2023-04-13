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
    public abstract class SwitchModeCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => $"Switch to {this.TargetMode} mode";
        public override ExecutorMode AvaliableIn => ExecutorMode.None;

        public abstract ExecutorMode TargetMode { get; }
        protected override void InnerExecute(CommandContext context)
        {
            context.Executor.Mode = TargetMode;
            if (TargetMode  == ExecutorMode.None)
                context.Terminal.Prompt = Terminal.Terminal.DefaultPrompt;
            else
                context.Terminal.Prompt = $"${this.TargetMode}> ";
        }
    }


    public class ListenerModeCommand : SwitchModeCommand
    {
        public override ExecutorMode AvaliableIn => ExecutorMode.All;
        public override string Name => "listener";
        public override ExecutorMode TargetMode => ExecutorMode.Listener;

    }

    public class AgentModeCommand : SwitchModeCommand
    {
        public override ExecutorMode AvaliableIn => ExecutorMode.All;
        public override string Name => "agent";
        public override ExecutorMode TargetMode => ExecutorMode.Agent;

    }

    //public class LauncherModeCommand : SwitchModeCommand
    //{
    //    public override ExecutorMode AvaliableIn => ExecutorMode.All;
    //    public override string Name => "launcher";
    //    public override ExecutorMode TargetMode => ExecutorMode.Launcher;

    //}
}
