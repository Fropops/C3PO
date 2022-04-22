using ApiModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
