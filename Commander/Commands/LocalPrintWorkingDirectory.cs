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
 
    public class PrintWorkingDirectoryCommand : EnhancedCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Print Commander Working Directory";
        public override string Name => "lpwd";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description);

        protected override async Task<bool> HandleCommand(CommandContext<EmptyCommandOptions> context)
        {
            context.Terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
