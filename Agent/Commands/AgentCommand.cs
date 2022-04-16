using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public abstract class AgentCommand
    {
        public virtual string Name { get; set; }

        public virtual void Execute(AgentTask task, Models.Agent agent, CommModule comm)
        {
            var result = new AgentTaskResult();
            result.Id = task.Id;
            try
            {
                this.InnerExecute(task, agent, result, comm);
            }
            catch(Exception e)
            {
                result.Result = "An unhandled error occured :" + Environment.NewLine;
                result.Result += e.ToString();
            }
            finally
            {
                result.Completion = 100;
                result.Completed = true;
                comm.SendResult(result);
            }

            
        }

        public abstract void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm);
    }
}
