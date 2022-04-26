﻿using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public TasksController(IAgentService agentService)
        {
            this._agentService = agentService;
        }

        [HttpGet]
        public IActionResult GetTasks()
        {
            var results = new List<AgentTaskResponse>();
            var agents = _agentService.GetAgents();

           foreach(var agent in agents)
            {
                foreach(var task in agent.TaskHistory)
                {
                    results.Add(new AgentTaskResponse()
                    {
                        AgentId = agent.Metadata.Id,
                        Arguments = task.Arguments,
                        Command = task.Command,
                        File = task.File,
                        Id = task.Id,
                        RequestDate = task.RequestDate,
                    });
                }
                
            }
            return Ok(results);
        }

        [HttpGet("results")]
        public IActionResult GetResults()
        {
            var results = new List<AgentTaskResultResponse>();
            var agents = _agentService.GetAgents();

            foreach (var agent in agents)
            {
                foreach (var res in agent.GetTaskResults())
                {
                    results.Add(new AgentTaskResultResponse()
                    {
                        Id = res.Id,
                        Result = res.Result,
                        Completed = res.Completed,
                        Completion = res.Completion,
                    });
                }

            }
            return Ok(results);
        }
    }
}