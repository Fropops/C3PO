using Agent.Communication;
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
        public AgentCommandContext ParentContext { get; set; }

        public Models.Agent Agent { get; set; }
        public IMessageService MessageService { get; set; }

        public IFileService FileService { get; set; }

        public IConfigService ConfigService { get; set; }
        public IProxyService ProxyService { get; set; }

        public AgentTaskResult Result { get; set; }


        public void AppendResult(string message, bool addEndLine = true)
        {
            if (string.IsNullOrEmpty(this.Result.Result))
                Result.Result = message;
            else
                Result.Result += message;

            if (addEndLine)
                Result.Result += Environment.NewLine;
        }

        public void Error(string message, bool addEndLine = false)
        {
            if (string.IsNullOrEmpty(this.Result.Error))
                Result.Error = message;
            else
                Result.Error += message;

            if (addEndLine)
                Result.Error += Environment.NewLine;
        }

        public void Objects<T>(T obj)
        {
            this.Result.Objects = Convert.ToBase64String(obj.Serialize());
        }
    }

    public abstract class AgentCommand
    {

        public AgentCommandContext Context { get; set; }

        protected bool SendMetadataWithResult = false;
        public virtual string Name { get; set; }

        public string Module => Assembly.GetExecutingAssembly().GetName().Name;

        public virtual void Execute(AgentTask task, AgentCommandContext context)
        {
            this.Context = context;
            context.Result.Id = task.Id;
            try
            {
                context.Result.Status = AgentResultStatus.Running;
                if (context.ParentContext == null) //sending will be handled in the composite command
                    context.MessageService.SendResult(context.Result);
                this.InnerExecute(task, context);
            }
            catch (Exception e)
            {
                //context.Result.Result = "An unhandled error occured :" + Environment.NewLine;
                //context.Result.Result += e.ToString();
#if DEBUG
                context.Result.Error = e.ToString();
#else
                context.Result.Error = e.Message;
#endif
            }
            finally
            {
                context.Result.Info = string.Empty;
                context.Result.Status = AgentResultStatus.Completed;
                if (context.ParentContext == null) //sending will be handled in the composite command
                    context.MessageService.SendResult(context.Result, this.SendMetadataWithResult);
            }

        }

        public abstract void InnerExecute(AgentTask task, AgentCommandContext context);

        public void Notify(AgentCommandContext context, string status)
        {
            if (context.ParentContext == null)
                context.MessageService.SendResult(new AgentTaskResult()
                {
                    Id = context.Result.Id,
                    Status = context.Result.Status,
                    Info = status
                });
            else
            {
                context.ParentContext.MessageService.SendResult(new AgentTaskResult()
                {
                    Id = context.ParentContext.Result.Id,
                    Status = context.ParentContext.Result.Status,
                    Info = status
                });
            }
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
