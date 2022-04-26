using ApiModels.Response;
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
        event EventHandler<ConnectionStatus> ConnectionStatusChanged;
        event EventHandler<List<AgentTask>> RunningTaskChanged;
        event EventHandler<AgentTaskResult> TaskResultUpdated;
        event EventHandler AgentsUpdated;
        Task Start();
        void Stop();
        void UpdateConfig();

        string ConnectAddress { get; set; }
        int ConnectPort { get; set; }

        List<Agent> GetAgents();

        Agent GetAgent(int index);
        Agent GetAgent(string id);
        IEnumerable<AgentTask> GetTasks(string id);
        AgentTask GetTask(string taskId);
        AgentTaskResult GetTaskResult(string taskId);
        Task<HttpResponseMessage> CreateListener(string name, int port);
        IEnumerable<Listener> GetListeners();
        Task<HttpResponseMessage> TaskAgent(string id, string cmd, string parms);

        Task<HttpResponseMessage> GetFileDescriptor(string filename);
        Task<HttpResponseMessage> GetFileChunk(string id, int chunkIndex);
        Task<HttpResponseMessage> GetFiles(string path);

        Task<HttpResponseMessage> PushFileChunk(FileChunckResponse chuck);
        Task<HttpResponseMessage> PushFileDescriptor(FileDescriptorResponse desc);

    }
}
