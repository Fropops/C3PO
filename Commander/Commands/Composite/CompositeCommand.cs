using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Executor;
using Common;
using Newtonsoft.Json;

namespace Commander.Commands.Composite
{
    public abstract class CompositeCommand<T> : EnhancedCommand<T>
    {
        protected List<AgentTask> Tasks { get; private set; } = new List<AgentTask>();
 
        protected void RegisterTask(string command, string arguments = null, string fileName = null, string fileId = null)
        {
            this.Tasks.Add(new AgentTask()
            {
                Id = Guid.NewGuid().ToString(),
                Command = command,
                Arguments = arguments,
                FileName = fileName,
                FileId = fileId
            });
        }

        protected byte[] GetTasksAsFile()
        {
            var camelSettings = new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            var json = JsonConvert.SerializeObject(this.Tasks, camelSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            this.Tasks.Clear();
            var agent = context.Executor.CurrentAgent;

            var result = await CreateComposition(context);
            if (!result)
                return false;


            var tasks = this.GetTasksAsFile();

            var fileName = "tasks";
            var fileId = await context.UploadAndDisplay(tasks, "fileName", "Uploading tasks");
            await context.CommModule.TaskAgentToDownloadFile(agent.Metadata.Id, fileId);

            await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), agent.Metadata.Id, "composite",fileId, fileName);


            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
            return true;
        }

        protected abstract Task<bool> CreateComposition(CommandContext<T> context);


        protected void Delay(int seconds)
        {
            this.RegisterTask(EndPointCommand.DELAY, seconds.ToString());
        }

        protected void Echo(string message)
        {
            this.RegisterTask(EndPointCommand.ECHO, message);
        }

        protected void Shell(string cmd)
        {
            this.RegisterTask(EndPointCommand.SHELL, cmd);
        }

        protected void Powershell(string cmd)
        {
            this.RegisterTask(EndPointCommand.POWERSHELL, cmd);
        }

        protected void StartPivot(ConnexionUrl endpoint)
        {
            this.RegisterTask(EndPointCommand.PIVOT, "start " + endpoint.ToString());
        }

        protected void StartPivot(string endpoint)
        {
            ConnexionUrl pt = ConnexionUrl.FromString(endpoint);
            this.StartPivot(pt);
        }

        protected void Dowload(string fileName, string fileId, string outFile = null)
        {
            this.RegisterTask(EndPointCommand.DOWNLOAD, outFile, fileName, fileId);
        }

    }

   
}
