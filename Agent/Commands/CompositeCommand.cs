using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class CompositeCommand : AgentCommand
    {
        //public List<AgentCommand> Commands { get; private set; } = new List<AgentCommand>();
        //public List<AgentTask> CommandTasks { get; private set; } = new List<AgentTask>();

        //public void RegisterTask(AgentTask t)
        //{
        //    this.CommandTasks.Add(t);
        //}
        public override string Name => "composite";

        public override void InnerExecute(AgentTask taskAgent, AgentCommandContext context)
        {
            this.CheckFileDownloaded(taskAgent, context);

            var file = context.FileService.ConsumeDownloadedFile(taskAgent.FileId);
            var json = file.GetFileContent();
            var comTasks = json.Deserialize<List<AgentTask>>();

            //var result = "Running " + this.Name + " (Composite Command)" + Environment.NewLine;
            var result = string.Empty;
            foreach(var task in comTasks)
            {
                if (string.IsNullOrEmpty(task.Command))
                    break;

                //result += "[>] Executing " + task.Command + " " + task.Arguments + Environment.NewLine;
                context.Result.Result = String.Empty;
                var t = context.Agent.HandleTask(task, context.Result, true);
                t.Join();
                result += context.Result.Result;
                if(!result.EndsWith(Environment.NewLine))
                    result += Environment.NewLine;

                //command generate an error => stop executing
                if(!string.IsNullOrEmpty(context.Result.Error))
                    break;

            }


            //override task id to link it o the original task (as it was overwritten in the execution of children commands)
            context.Result.Id = taskAgent.Id;
            context.Result.Result = result;
        }
    }
}
