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
        private readonly Dictionary<string, AgentTaskResult> _results = new();

        public void AddTaskResult(AgentTaskResult res)
        {
            if (!_results.ContainsKey(res.Id))
                _results.Add(res.Id, res);
            else
            {
                var existing = _results[res.Id];
                existing.Status = res.Status;
                existing.Output += res.Output;
                existing.Error = res.Error;
                existing.Info = res.Info;
                existing.Objects = res.Objects;
            }
        }

        public AgentTaskResult GetAgentTaskResult(string id)
        {
            if (!_results.ContainsKey(id))
                return null;
            return _results[id];
        }

        public IEnumerable<AgentTaskResult> GetAgentTaskResults()
        {
            return _results.Values;
        }

        public void RemoveAgentTaskResults(AgentTaskResult res)
        {
            _results.Remove(res.Id);
        }
    }
}
