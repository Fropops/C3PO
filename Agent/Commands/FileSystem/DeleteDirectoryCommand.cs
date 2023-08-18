﻿using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DeleteDirectoryCommand : AgentCommand
    {
        public override CommandId Command => CommandId.RmDir;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);
            var path = task.GetParameter<string>(ParameterId.Path);
            bool recurse = false;
            if (task.HasParameter(ParameterId.Recursive) && task.GetParameter<bool>(ParameterId.Recursive))
                recurse = true;


            Directory.Delete(path, recurse);
            context.AppendResult($"Folder {path} removed");
        }
    }
}
