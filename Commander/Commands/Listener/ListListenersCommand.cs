using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class ListListenersCommandOptions
    {
        public bool verbose { get; set; }
    }

    public class ListListenersCommand : EnhancedCommand<ListListenersCommandOptions>
    {
        public override string Description => "List all listeners";
        public override string Name => "list";

        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(ListListenersCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var results = new SharpSploitResultList<ListListenersResult>();

            if (options.verbose)
                terminal.WriteLine("Hello from verbose output!");

            var result = comm.GetListeners();
            if (result.Count() == 0)
            {
                terminal.WriteInfo("No Listeners running.");
                return true;
            }

            var index = 0;
            foreach (var listener in result)
            {
                results.Add(new ListListenersResult()
                {
                    Index = index,
                    Name = listener.Name,
                    BindPort = listener.BindPort,
                });

                index++;
            }

            terminal.WriteLine(results.ToString());

            return true;
        }


        public sealed class ListListenersResult : SharpSploitResult
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public int BindPort { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Index), Value = Index },
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(BindPort), Value = BindPort },
            };
        }
    }
}
