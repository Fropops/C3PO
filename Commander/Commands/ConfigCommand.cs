using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class ConfigCommandOptions
    {
        public string server { get; set; }
        public int? port { get; set; }

        public bool NoOptions
        {
            get
            {
                return String.IsNullOrEmpty(server)
                    && !port.HasValue;
            }
        }
    }

    public class ConfigCommand : EnhancedCommand<ConfigCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List & Change Commander settings";
        public override string Name => "config";

        public override ExecutorMode AvaliableIn => ExecutorMode.None;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option<string>(new[] { "--server", "-s" }, "The host name or ip."),
                new Option<int?>(new[] { "--port", "-p" }, "The host listening port."),

            };

        protected async override Task<bool> HandleCommand(CommandContext<ConfigCommandOptions> context)
        {
            if (context.Options.NoOptions)
            {
                var results = new SharpSploitResultList<ConfigResult>();
                results.Add(new ConfigResult() { Name = "Server", Value = context.CommModule.ConnectAddress });
                results.Add(new ConfigResult() { Name = "Port", Value = context.CommModule.ConnectPort.ToString() });
                results.Add(new ConfigResult() { Name = "TmpFolder", Value = InjectCommand.TmpFolder.ToString() });
                results.Add(new ConfigResult() { Name = "ModuleFolder", Value = InjectCommand.ModuleFolder.ToString() });
                context.Terminal.WriteLine(results.ToString());
                return true;
            }
            else
            {
                bool netConfigChanged = false;
                if (!string.IsNullOrEmpty(context.Options.server))
                {
                    context.CommModule.ConnectAddress = context.Options.server;
                    netConfigChanged = true;
                    context.Terminal.WriteSuccess($"Server changed to {context.Options.server}.");
                }
                if (context.Options.port.HasValue)
                {
                    context.CommModule.ConnectPort = context.Options.port.Value;
                    netConfigChanged = true;
                    context.Terminal.WriteSuccess($"Server port changed to {context.Options.port.Value}.");
                }
                if (netConfigChanged)
                    context.CommModule.UpdateConfig();
            }
            return true;
        }
        
    }

    public sealed class ConfigResult : SharpSploitResult
    {
        public string Name { get; set; }
        public string Value { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Value), Value = Value },
            };
    }
}
