using ApiModels.Response;
using Commander.Commands;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    public class GetCommandOptions
    {
        public string remotefile { get; set; }

        public string outfile { get; set; }
        public bool verbose { get; set; }
    }
    public class GetCommand : EnhancedCommand<GetCommandOptions>
    {
        public override string Description => "Download a ile from the TeamServer";
        public override string Name => "get";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("remotefile", "name of the file to download"),
                new Argument<string>( "outFile", () => string.Empty, "local file name to be saved to."),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        protected override async Task<bool> HandleCommand(GetCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {
            var path = options.remotefile;
            if (executor.Mode == ExecutorMode.AgentInteraction && !path.StartsWith("/"))
            {
                path = "Agent/" + executor.CurrentAgent.Metadata.Id  + "/" + path;
            }


            var result = await comm.GetFileDescriptor(path);
            if (!result.IsSuccessStatusCode)
            {
                if(result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    terminal.WriteError($"File {options.remotefile} not found!");
                else
                    terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var desc = JsonConvert.DeserializeObject<FileDescriptorResponse>(json);

            var chunks = new List<FileChunckResponse>();

            for (int index = 0; index < desc.ChunkCount; ++index)
            {
                result = await comm.GetFileChunk(desc.Id, index);
                if (!result.IsSuccessStatusCode)
                {
                    terminal.WriteError("An error occured : " + result.StatusCode);
                    return false;
                }

                json = await result.Content.ReadAsStringAsync();
                var chunk = JsonConvert.DeserializeObject<FileChunckResponse>(json);
                chunks.Add(chunk);
            }

            if(!string.IsNullOrEmpty(options.outfile))
            {
                path = options.outfile;
            }
            else
            {
                path = Path.Combine(Environment.CurrentDirectory, desc.Name);
            }

            using (FileStream fs= new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                foreach (var chunk in chunks.OrderBy(c => c.Index))
                {
                    var bytes = Convert.FromBase64String(chunk.Data);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            terminal.WriteSuccess($"File dowloaded to {path}.");

            return true;

        }
    }
}
