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

    public class DownloadCommandOptions
    {
        public string remotefile { get; set; }
        public string localfile { get; set; }
    }
    public class DownloadCommand : EndPointCommand<DownloadCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Download a file from the agent";
        public override string Name => "download";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("remotefile",  "name of the file to load from the agent"),
                new Argument<string>("localFile",() => string.Empty, "local file name to downloaded to."),
            };

        protected async override Task<bool> HandleCommand(CommandContext<DownloadCommandOptions> context)
        {

            var fileName = context.Options.remotefile;
            string dest = Path.GetFileName(fileName);
            if(!string.IsNullOrEmpty(context.Options.localfile))
            {
                dest = context.Options.localfile;
            }
                

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, EndPointCommand.UPLOAD, $"{fileName} {dest}");
           
            context.Terminal.WriteInfo($"Agent tasked to upload file to server.");

            return true;
        }
    }
}
