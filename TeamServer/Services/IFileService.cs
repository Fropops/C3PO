using ApiModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.File;

namespace TeamServer.Services
{
    public interface IFileService
    {
        
        FileDescriptor GetFile(string id);
 
        void AddFile(FileDescriptor desc);

        void CleanDownloaded();


        string GetFullPath(string fileName);
        string GetAgentPath(string agentId, string fileName);

        string GetAgentPath(string agentId);

        public string GetListenerPath(string listenerName, string fileName);

        public string GetListenerPath(string listenerName);

        void SaveResults(Agent agent, IEnumerable<AgentTaskResult> results);

        public List<AgentFileChunck> GetFileChunksForAgent(string id);

        public void AddAgentFileChunk(AgentFileChunck chunk);
    }
}
