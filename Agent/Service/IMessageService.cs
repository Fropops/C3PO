using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IMessageService
    {
        AgentMetadata AgentMetaData { get; }
        void EnqueueTask(MessageTask task);
        void EnqueueTasks(IEnumerable<MessageTask> tasks);
        void EnqueueResults(IEnumerable<MessageResult> results);
        List<MessageTask> GetMessageTasksForAgent(string id);
        List<MessageTask> GetMessageTasksToRelay(List<string> agentIds);
        List<MessageResult> GetMessageResultsToRelay();


        void SendResult(AgentTaskResult res, bool includeMetaData = false);
    }
}
