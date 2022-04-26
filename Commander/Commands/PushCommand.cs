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
    public class PushCommandOptions
    {
        public string remotefile { get; set; }

        public string localfile { get; set; }
        public bool verbose { get; set; }
    }
    public class PushCommand : EnhancedCommand<PushCommandOptions>
    {
        public override string Description => "Upload a file to the TeamServer";
        public override string Name => "push";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>( "localFile", "local file name to be pushed to the server."),
                new Argument<string>("remotefile", () => string.Empty, "name of the file to saved on the server"),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };

        public const int ChunkSize = 10000;

        protected override async Task<bool> HandleCommand(PushCommandOptions options, ITerminal terminal, IExecutor executor, ICommModule comm)
        {

            var path = options.localfile;
            var filename = Path.GetFileName(path);
            if (!File.Exists(path))
            {
                terminal.WriteError("File {file} does not exists!");
                return false;
            }


            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            if (!string.IsNullOrEmpty(options.remotefile))
            {
                filename = options.remotefile;
            }


            var desc = new FileDescriptorResponse()
            {
                Length = fileBytes.Length,
                ChunkSize = ChunkSize,
                Id = Guid.NewGuid().ToString(),
                Name = filename
            };

            var chunks = new List<FileChunckResponse>();

            int index = 0;
            using (var ms = new MemoryStream(fileBytes))
            {

                var buffer = new byte[ChunkSize];
                int numBytesToRead = (int)ms.Length;

                while (numBytesToRead > 0)
                {

                    int n = ms.Read(buffer, 0, ChunkSize);
                    //var data =
                    var chunk = new FileChunckResponse()
                    {
                        FileId = desc.Id,
                        Data = System.Convert.ToBase64String(buffer.Take(n).ToArray()),
                        Index = index,
                    };
                    chunks.Add(chunk);
                    numBytesToRead -= n;

                    index++;
                }
            }

            desc.ChunkCount = chunks.Count;

            var result = await comm.PushFileDescriptor(desc);
            if (!result.IsSuccessStatusCode)
            {
                var cont = await result.Content.ReadAsStringAsync();
                terminal.WriteError($"An error occured : {result.StatusCode} - {cont}");
                return false;
            }

            index = 0;
            foreach (var chunk in chunks)
            {
                result = await comm.PushFileChunk(chunk);
                if (!result.IsSuccessStatusCode)
                {
                    var cont = await result.Content.ReadAsStringAsync();
                    terminal.WriteError($"An error occured : {result.StatusCode} - {cont}");
                    return false;
                }
                //OnCompletionChanged?.Invoke(index * 100 / desc.ChunkCount);
                index++;
            }

            terminal.WriteSuccess($"File uploaded to {filename}.");

            return true;

        }
    }
}
