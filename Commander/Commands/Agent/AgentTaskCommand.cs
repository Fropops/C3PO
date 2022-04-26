﻿using ApiModels.Response;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
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

        protected override void InnerExecute(ITerminal terminal, IExecutor executor, ICommModule comm, string parms)
        {
            var agent = executor.CurrentAgent;
            var response = comm.TaskAgent(agent.Metadata.Id, this.Name, parms).Result;
            if (!response.IsSuccessStatusCode)
            {
                terminal.WriteError("An error occured : " + response.StatusCode);
                return;
            }

            terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");
        }
    }



}