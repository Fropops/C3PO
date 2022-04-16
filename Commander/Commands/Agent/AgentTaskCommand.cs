using ApiModels.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{


    public class AgentTaskCommand : ExecutorCommand
    {

        public AgentTaskCommand()
        {
            this.Name = "dummyTask";
        }
        public AgentTaskCommand(string name)
        {
            this.Name = name;
        }
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        protected override void InnerExecute(Executor executor, string parms)
        {
            var agent = executor.CurrentAgent;
            var response = executor.CommModule.TaskAgent(agent.Metadata.Id, this.Name, parms).Result;
            if (!response.IsSuccessStatusCode)
            {
                Terminal.WriteError("An error occured : " + response.StatusCode);
                return;
            }

            Terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }
    }



}
