﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{

    public interface IAgentService
    {
        void AddAgent(Agent agent);
        IEnumerable<Agent> GetAgents();
        Agent GetAgent(string id);
        void RemoveAgent(Agent agent);
        List<Agent> GetAgentToRelay(string id);
    }
    public class AgentService : IAgentService
    {
        private readonly List<Agent> _agents = new();

        public void AddAgent(Agent agent)
        {
            _agents.Add(agent);
        }

        public Agent GetAgent(string id)
        {
            return GetAgents().FirstOrDefault(a => a.Id.Equals(id));
        }

        public List<Agent> GetAgentToRelay(string id)
        {
            return GetAgents().Where(a => a.Id == id || a.RelayId == id).ToList();
        }

        public IEnumerable<Agent> GetAgents()
        {
            return _agents;
        }

        public void RemoveAgent(Agent agent)
        {
            _agents.Remove(agent);
        }
    }
}
