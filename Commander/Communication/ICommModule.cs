﻿using ApiModels.Response;
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
        event EventHandler<string> AgentAdded;
        Task Start();
        void Stop();
        void UpdateConfig();

        string ConnectAddress { get; set; }
        int ConnectPort { get; set; }

        List<Agent> GetAgents();

        Agent GetAgent(int index);
        Agent GetAgent(string id);

        Task<HttpResponseMessage> StopAgent(string id);
        IEnumerable<AgentTask> GetTasks(string id);
        AgentTask GetTask(string taskId);
        AgentTaskResult GetTaskResult(string taskId);
        Task<HttpResponseMessage> CreateListener(string name, int port);
        IEnumerable<Listener> GetListeners();
        Task TaskAgent(string label, string taskId, string agentId, string cmd, string parms = null);
        Task TaskAgent(string label, string taskId, string agentId, string cmd, string fileId, string fileName, string parms = null);

        Task<Byte[]> Download(string id, Action<int> OnCompletionChanged = null);
       
        Task<string> Upload(byte[] fileBytes, string filename, Action<int> OnCompletionChanged = null);

    }
}
