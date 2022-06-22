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
            var results = new SharpSploitResultList<ListDirectoryResult>();

            var list = new List<ListDirectoryResult>();
            string path = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(context.Options.path))
            {
                path = context.Options.path;
            }

            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                list.Add(new ListDirectoryResult()
                {
                    Name = dirInfo.Name,
                    Length = 0,
                    IsFile= false,
                });
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                list.Add(new ListDirectoryResult()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length,
                    IsFile= true
                });
            }


            results.AddRange(list.OrderBy(f => f.IsFile).ThenBy(f => f.Name));
            context.Terminal.WriteLine(results.ToString());
            return true;
        }

        public sealed class ListDirectoryResult : SharpSploitResult
        {
            public long Length { get; set; }
            public string Name { get; set; }
            public bool IsFile { get; set; }

            public string Type
            {
                get
                {
                    return IsFile ? "File" : "Folder";
                }
            }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Type), Value = Type },
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Length), Value = Length },
            };
        }
    }
}
