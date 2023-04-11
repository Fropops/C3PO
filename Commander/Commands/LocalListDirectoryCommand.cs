using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using Spectre.Console;
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
    public class LocalListDirectoryCommandOptions
    {
        public string path { get; set; }
    }

    public class LocalListDirectoryCommand : EnhancedCommand<LocalListDirectoryCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List Directory";
        public override string Name => "lls";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path" ,() => string.Empty, "directory to list"),
            };


        protected override async Task<bool> HandleCommand(CommandContext<LocalListDirectoryCommandOptions> context)
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Name").LeftAligned());
            table.AddColumn(new TableColumn("Length").LeftAligned());
            table.AddColumn(new TableColumn("IsFile").LeftAligned());

            string path = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(context.Options.path))
            {
                path = context.Options.path;
            }

            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                table.AddRow(
                    dirInfo.Name,
                    0.ToString(),
                    "No"
                );
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                table.AddRow(
                    Path.GetFileName(fileInfo.FullName),
                    fileInfo.Length.ToString(),
                    "Yes"
                );
            }

            context.Terminal.Write(table);
            return true;
        }
    }
}
