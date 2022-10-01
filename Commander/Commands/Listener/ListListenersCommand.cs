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
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all listeners";
        public override string Name => "list";

        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ListListenersCommandOptions> context)
        {
            var results = new SharpSploitResultList<ListListenersResult>();

            if (context.Options.verbose)
                context.Terminal.WriteLine("Hello from verbose output!");

            var result = context.CommModule.GetListeners();
            if (result.Count() == 0)
            {
                context.Terminal.WriteInfo("No Listeners running.");
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
                    PublicPort = listener.PublicPort,
                    Address = listener.Ip,
                    Id = listener.Id,
                    Secured = listener.Secured ? "Yes" : "No",
                });

                index++;
            }

            context.Terminal.WriteLine(results.ToString());

            return true;
        }


        public sealed class ListListenersResult : SharpSploitResult
        {
            public int Index { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public int BindPort { get; set; }
            public int PublicPort { get; set; }
            public string Secured { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Index), Value = Index },
                //new SharpSploitResultProperty { Name = nameof(Id), Value = Id },
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Address), Value = Address },
                new SharpSploitResultProperty { Name = nameof(BindPort), Value = BindPort },
                new SharpSploitResultProperty { Name = nameof(Secured), Value = Secured },
                new SharpSploitResultProperty { Name = nameof(PublicPort), Value = PublicPort },
            };
        }
    }
}
