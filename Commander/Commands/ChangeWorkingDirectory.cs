using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class ChangeWorkingDirectoryCommandOptions
    {
        public string path { get; set; }
    }

    public class ChangeWorkingDirectoryCommand : EnhancedCommand<ChangeWorkingDirectoryCommandOptions>
    {
        public override string Description => "Change Commander Working Directory";
        public override string Name => "cwd";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path" ,() => string.Empty, "path on the server to list from."),
            };


        protected override async Task<bool> HandleCommand(ChangeWorkingDirectoryCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            if(!string.IsNullOrEmpty(options.path))
            {
                Directory.SetCurrentDirectory(options.path);
            }

            terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
