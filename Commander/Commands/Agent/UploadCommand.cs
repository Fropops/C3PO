using ApiModels.Response;
using Commander.Executor;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{

    public class UploadCommandoptions
    {
        public string remotefile { get; set; }
        public string localfile { get; set; }
    }
    public class UploadCommand : EndPointCommand<UploadCommandoptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Upload a file to the agent";
        public override string Name => "upload";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>( "localFile", "local file name to be uploaded to the agent."),
                new Argument<string>("remotefile", () => string.Empty, "name of the file to be saved on the agent"),
            };

        protected async override Task<bool> HandleCommand(CommandContext<UploadCommandoptions> context)
        {
            var path = context.Options.localfile;
            var filename = Path.GetFileName(path);
            if (!File.Exists(path))
            {
                context.Terminal.WriteError("File {file} does not exists!");
                return false;
            }


            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            if (!string.IsNullOrEmpty(context.Options.remotefile))
            {
                filename = context.Options.remotefile;
            }

            var fileId = await context.CommModule.Upload(fileBytes, filename, a =>
            {
                context.Terminal.WriteLine($"uploading {filename} ({a}%)");
            });

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, EndPointCommand.DOWNLOAD, fileId, filename);
           
            context.Terminal.WriteInfo($"File uploaded to the server, agent tasked to download the file.");

            return true;
        }
    }
}
