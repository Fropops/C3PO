﻿using Agent.Communication;
using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class AgentCommandContext
    {
        public Models.Agent Agent { get; set; }
        public IMessageService MessageService { get; set; }

        public CommModule commModule { get; set; }
        public IFileService FileService { get; set; }

        public IProxyService ProxyService { get; set; }

        public AgentTaskResult Result { get; set; }
    }

    public abstract class AgentCommand
    {

        protected bool SendMetadataWithResult = false;
        public virtual string Name { get; set; }

        public bool IsSubCommand { get; set; } = false;

        public string Module => Assembly.GetExecutingAssembly().GetName().Name;

        public virtual void Execute(AgentTask task, AgentCommandContext context)
        {
            context.Result.Id = task.Id;
            try
            {
                context.Result.Status = AgentResultStatus.Running;
                if (!this.IsSubCommand) //sending will be handled in the composite command
                    context.MessageService.SendResult(context.Result);
                this.InnerExecute(task, context);
            }
            catch (Exception e)
            {
                context.Result.Result = "An unhandled error occured :" + Environment.NewLine;
                context.Result.Result += e.ToString();
                context.Result.Error = e.ToString();
            }
            finally
            {
                context.Result.Info = string.Empty;
                context.Result.Status = AgentResultStatus.Completed;
                if (!this.IsSubCommand) //sending will be handled in the composite command
                    context.MessageService.SendResult(context.Result, this.SendMetadataWithResult);
            }

        }

        public abstract void InnerExecute(AgentTask task, AgentCommandContext context);

        public void Notify(AgentCommandContext context, string status)
        {
            context.Result.Info = status;
            if (!this.IsSubCommand) //sending will be handled in the composite command
                context.MessageService.SendResult(context.Result);
        }


        protected void CheckFileDownloaded(AgentTask task, AgentCommandContext context)
        {
            int percent = -1;
            while (!context.FileService.IsDownloadComplete(task.FileId))
            {
                var newpercent = context.FileService.GetDownloadPercent(task.FileId);
                if (newpercent != percent)
                {
                    percent = newpercent;
                    this.Notify(context, $"{task.FileName} Downloading {percent}%");
                }

                if (percent != 100)
                    Thread.Sleep(context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval);
            }

            this.Notify(context, $"{task.FileName} Downloaded");

        }

        protected void CheckFileUploaded(string fileId, string fileName, AgentCommandContext context)
        {
            int percent = -1;
            while (!context.FileService.IsUploadComplete(fileId))
            {
                var newpercent = context.FileService.GetUploadPercent(fileId);
                if (newpercent != percent)
                {
                    percent = newpercent;
                    this.Notify(context, $"{fileName} uploading {percent}%");
                }

                if (percent != 100)
                    Thread.Sleep(context.Agent.Communicator.MessageService.AgentMetaData.SleepInterval);
            }

            this.Notify(context, $"{fileName} uploaded");

        }
    }
}
