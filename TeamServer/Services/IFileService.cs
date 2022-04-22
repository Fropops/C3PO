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
        FileDescriptor GetFile(string id);
        FileDescriptor CreateFileDescriptor(string filePath);
        void CleanDownloaded();
    }
}
