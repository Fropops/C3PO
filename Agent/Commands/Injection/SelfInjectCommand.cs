using Agent.Internal;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class SelfInjectCommand : AgentCommand
    {
        public override string Name => "inject-self";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            throw new NotImplementedException();
            var fileName = task.FileId;
            //var fileContent = commm.Download(task.FileId, a =>
            //{
            //    result.Info = $"Downloading {fileName} ({a}%)";
            //    commm.SendResult(result);
            //}).Result;

            //this.Notify(result, commm, $"{fileName} Downloaded");

            //var shellcode = fileContent;

            //var injectRes =  Injector.InjectSelfWithOutput(fileContent);
            //if(!injectRes.Succeed)
            //    result.Result += $"Injection failed : {injectRes.Error}";
            //else
            //{
            //    result.Result += $"Injection succeed!" + Environment.NewLine;
            //    if (!string.IsNullOrEmpty(injectRes.Output))
            //        result.Result += injectRes.Output;
            //}
        }
    }
}
