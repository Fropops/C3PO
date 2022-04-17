using Commander.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Communication
{
    public interface ICommModule
    {
        Task Start();
        void Stop();
        void UpdateConfig();

        List<Agent> GetAgents();

        Agent GetAgent(int index);
        Agent GetAgent(string id);
        IEnumerable<AgentTask> GetTasks(string id);
        AgentTask GetTask(string taskId);
        AgentTaskResult GetTaskResult(string taskId);
        Task<HttpResponseMessage> CreateListener(string name, int port);
        IEnumerable<Listener> GetListeners();
        Task<HttpResponseMessage> TaskAgent(string id, string cmd, string parms);

    }
}
