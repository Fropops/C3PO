using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models.File;

namespace TeamServer.Services
{
    public class FileService : IFileService
    {
        public static Dictionary<string, FileDescriptor> DownloadCache = new Dictionary<string, FileDescriptor>();
        public const int ChunkSize = 10240; //10kB

        public string GetFullPath(FileType filetype, string fileName)
        {
            string path;
            switch (filetype)
            {
                case FileType.Custom:
                    {
                        path = @"e:\Share\tmp\Custom\";
                    }
                    break;
                case FileType.Assembly:
                    {
                        path = @"e:\Share\tmp\Assembly\";
                    }
                    break;

                default:
                    return null;
            }

            return Path.Combine(path, fileName);
        }

        public FileDescriptor GetFile(string id)
        {
            if (!DownloadCache.ContainsKey(id))
                return null;

            return DownloadCache[id];
        }
        public FileDescriptor CreateFileDescriptor(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var desc = new FileDescriptor()
            {
                Name = Path.GetFileName(filePath),
                Id = Guid.NewGuid().ToString(),
                ChunkSize = FileService.ChunkSize,
            };

            
            using (var fs = File.OpenRead(filePath))
            {
                desc.Length = fs.Length;

               
                int index = 0;
                var buffer = new byte[FileService.ChunkSize];
                int numBytesToRead = (int)fs.Length;

                while (numBytesToRead > 0)
                {
                    
                    int n = fs.Read(buffer, 0, FileService.ChunkSize);
                    //var data =
                    var chunk = new FileChunk()
                    {
                        FileId = desc.Id,
                        Data = System.Convert.ToBase64String(buffer),
                        Index = index,
                    };
                    desc.Chunks.Add(chunk);
                    numBytesToRead -= n;

                    index++;
                }
            }

            desc.ChunkCount = desc.Chunks.Count;

            DownloadCache.Add(desc.Id, desc);

            return desc;
        }

        public void CleanDownloaded()
        {
            foreach(var id in DownloadCache.Keys.ToList())
            {
                if(DownloadCache[id].IsDownloaded)
                {
                    DownloadCache.Remove(id);
                }
            }
        }
    }
}
