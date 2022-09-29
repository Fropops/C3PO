using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public abstract class SimpleEndPointCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Core;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        protected override void InnerExecute(CommandContext context)
        {
            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, context.CommandParameters).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.ShortId}.");
        }
    }

    public class ShellCommand : SimpleEndPointCommand
    {
        public override string Description => "Send a command to be executed by the agent shell";
        public override string Name => EndPointCommand.SHELL;
    }

    public class PowerShellCommand : SimpleEndPointCommand
    {
        public override string Description => "Send a command to be executed by the agent shell";
        public override string Name => EndPointCommand.POWERSHELL;
    }

    public class RunCommand : SimpleEndPointCommand
    {
        public override string Description => "Run an executable/command on the agent";
        public override string Name => EndPointCommand.RUN;
    }

    public class StartCommand : SimpleEndPointCommand
    {
        public override string Description => "Start an executable/command on the agent";
        public override string Name => EndPointCommand.START;
    }

    public class CATCommand : SimpleEndPointCommand
    {
        public override string Description => "Display the content of a file";
        public override string Name => EndPointCommand.CAT;
    }

}
