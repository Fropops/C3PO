using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public abstract class AgentCommand
    {
        public virtual string Name { get; set; }

        public string Module => Assembly.GetExecutingAssembly().GetName().Name;

        protected bool PreventTaskCompletion = false;
        public virtual void Execute(AgentTask task, Models.Agent agent, MessageManager comm)
        {
            var result = new AgentTaskResult();
            result.Id = task.Id;
            try
            {
                result.Status = AgentResultStatus.Running;
                comm.SendResult(result);
                this.InnerExecute(task, agent, result, comm);
            }
            catch(Exception e)
            {
                result.Result = "An unhandled error occured :" + Environment.NewLine;
                result.Result += e.ToString();
            }
            finally
            {
                result.Info = string.Empty;
                if(!this.PreventTaskCompletion)
                    result.Status = AgentResultStatus.Completed;
                comm.SendResult(result);
            }

            
        }

        public abstract void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm);

        public void Notify(AgentTaskResult result, MessageManager comm, string status)
        {
            result.Info = status;
            comm.SendResult(result);
        }
    }
}
