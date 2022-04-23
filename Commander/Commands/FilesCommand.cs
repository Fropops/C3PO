using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class FilesCommandOptions
    {
        public string path { get; set; }
    }

    public class FilesCommand : EnhancedCommand<FilesCommandOptions>
    {
        public override string Description => "List files on  the server";
        public override string Name => "files";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path" ,() => " ", "path on the server to list from."),
            };


        protected override async Task<bool> HandleCommand(FilesCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var results = new SharpSploitResultList<FilesCommandResult>();


            //if (options.path == "/")
            //    options.path = " ";

            var path = options.path;
            if (executor.Mode == ExecutorMode.AgentInteraction && !path.StartsWith("/"))
            {
                path = "Agent/" + executor.CurrentAgent.Metadata.Id + path;
            }

                var result = await comm.GetFiles(path);
            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    terminal.WriteError($"Path {options.path} not found!");
                else
                    terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var files = JsonConvert.DeserializeObject<FileFolderListResponse[]>(json);

            


            if (string.IsNullOrEmpty(path.Trim()))
                path = "/";

            terminal.WriteSuccess($"Content of {path}");
            var index = 0;
            foreach (var file in files.OrderBy(f => f.IsFile).ThenBy(f => f.Name))
            {
                results.Add(new FilesCommandResult()
                {
                    Length = file.Length,
                    Name = file.Name,
                    IsFile = file.IsFile,
                });

                index++;
            }

            terminal.WriteLine(results.ToString());

            return true;
        }


        public sealed class FilesCommandResult : SharpSploitResult
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
