using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models.File;

namespace TeamServer.Services
{
    public enum FileType
    {
        Custom = 0,
        Assembly = 1,
        PE = 2,
    }

    public interface IFileService
    {
        string GetFullPath(FileType filetype, string fileName);
        FileDescriptor GetFile(string id);
        FileDescriptor CreateFileDescriptor(string filePath);
        void CleanDownloaded();
    }
}
