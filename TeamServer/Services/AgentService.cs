using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels;
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
        Agent GetOrCreateAgent(string agentId);
    }
    public class AgentService : IAgentService
    {
        private readonly IChangeTrackingService _changeTrackingService;
        public AgentService(IChangeTrackingService changeTrackingService)
        {
            _changeTrackingService = changeTrackingService;
        }

        private readonly Dictionary<string, Agent> _agents = new();

        public void AddAgent(Agent agent)
        {
            if (!_agents.ContainsKey(agent.Id))
                _agents.Add(agent.Id, agent);
            else
                _agents[agent.Id] = agent;
        }

        public Agent GetAgent(string id)
        {
            if (!_agents.ContainsKey(id))
                return null;
            return _agents[id];
        }

        public List<Agent> GetAgentToRelay(string id)
        {
            return GetAgents().Where(a => a.Id == id || a.RelayId == id).ToList();
        }

        public IEnumerable<Agent> GetAgents()
        {
            return _agents.Values;
        }

        public void RemoveAgent(Agent agent)
        {
            _agents.Remove(agent.Id);
        }

        public Agent GetOrCreateAgent(string agentId)
        {
            var agent = this.GetAgent(agentId);
            if (agent == null)
            {
                agent = new Agent(agentId);
                this.AddAgent(agent);
                this._changeTrackingService.TrackChange(ChangingElement.Agent, agentId);
            }
            return agent;
        }
    }
}
