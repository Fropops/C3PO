using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using TeamServer.Models;

namespace TeamServer.Services
{

    public interface IAgentTaskResultService
    {
        void AddTaskResult(AgentTaskResult res);
        IEnumerable<AgentTaskResult> GetAgentTaskResults();
        AgentTaskResult GetAgentTaskResult(string id);
        void RemoveAgentTaskResults(AgentTaskResult res);
    }
    public class AgentTaskResultService : IAgentTaskResultService
    {
        private readonly List<AgentTaskResult> _results = new();

        public void AddTaskResult(AgentTaskResult res)
        {
            _results.Add(res);
        }

        public AgentTaskResult GetAgentTaskResult(string id)
        {
            return GetAgentTaskResults().FirstOrDefault(a => a.Id == id);
        }

        public IEnumerable<AgentTaskResult> GetAgentTaskResults()
        {
            return _results;
        }

        public void RemoveAgentTaskResults(AgentTaskResult res)
        {
            _results.Remove(res);
        }
    }
}
