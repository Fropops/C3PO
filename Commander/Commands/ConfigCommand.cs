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
        public override string Description => "List & Change Commander settings";
        public override string Name => "config";

        public override ExecutorMode AvaliableIn => ExecutorMode.None;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option<string>(new[] { "--server", "-s" }, "The host name or ip."),
                new Option<int?>(new[] { "--port", "-p" }, "The host listening port."),

            };

        protected async override Task<bool> HandleCommand(ConfigCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            if (options.NoOptions)
            {
                var results = new SharpSploitResultList<ConfigResult>();
                results.Add(new ConfigResult() { Name = "Server", Value = comm.ConnectAddress });
                results.Add(new ConfigResult() { Name = "Port", Value = comm.ConnectPort.ToString() });
                terminal.WriteLine(results.ToString());
                return true;
            }
            else
            {
                bool netConfigChanged = false;
                if (!string.IsNullOrEmpty(options.server))
                {
                    comm.ConnectAddress = options.server;
                    netConfigChanged = true;
                    terminal.WriteSuccess($"Server changed to {options.server}.");
                }
                if (options.port.HasValue)
                {
                    comm.ConnectPort = options.port.Value;
                    netConfigChanged = true;
                    terminal.WriteSuccess($"Server port changed to {options.port.Value}.");
                }
                if (netConfigChanged)
                    comm.UpdateConfig();
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
