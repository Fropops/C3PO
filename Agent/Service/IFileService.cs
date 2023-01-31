using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IFileService
    {
        bool IsDownloadComplete(string fileId);
        int GetDownloadPercent(string fileId);

        File ConsumeDownloadedFile(string fileId);
        bool IsUploadComplete(string fileId);
        int GetUploadPercent(string fileId);
        void AddFileChunck(FileChunk chunk);
        void AddFileToUpload(string id, string fileName, byte[] data);
        FileChunk GetChunkToSend();
    }
}
