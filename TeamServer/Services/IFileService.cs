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
        string GetFullPath(string fileName);
        FileDescriptor GetFileToDownload(string id);
        FileDescriptor GetFileToUpload(string id);
        FileDescriptor CreateFileDescriptor(string filePath);

        void AddFileToUpload(FileDescriptor desc);
        void CleanDownloaded();

        void CleanUploaded();

        List<FileFolderListResponse> List(string fullPath);

        void SaveUploadedFile(FileDescriptor desc, string path);

        string GetAgentPath(string agentId, string fileName);

        string GetAgentPath(string agentId);

        public string GetListenerPath(string listenerName, string fileName);

        public string GetListenerPath(string listenerName);

        void SaveResults(Agent agent, IEnumerable<AgentTaskResult> results);
    }
}
