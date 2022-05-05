using ApiModels.Response;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.File;

namespace TeamServer.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static Dictionary<string, FileDescriptor> DownloadCache = new Dictionary<string, FileDescriptor>();
        public static Dictionary<string, FileDescriptor> UploadCache = new Dictionary<string, FileDescriptor>();
        public const int ChunkSize = 10240; //10kB

        public string GetFullPath(string fileName)
        {
            var root = _configuration.GetValue<string>("FileSystemRoot");

            var actualPath = Path.Combine(root, fileName.Replace("..", string.Empty));
            if (!actualPath.StartsWith(root))
                actualPath = Path.Combine(root, Path.GetFileName(fileName));
            return actualPath;
        }



        public FileDescriptor GetFileToDownload(string id)
        {
            if (!DownloadCache.ContainsKey(id))
                return null;

            return DownloadCache[id];
        }

        public FileDescriptor GetFileToUpload(string id)
        {
            if (!UploadCache.ContainsKey(id))
                return null;

            return UploadCache[id];
        }

        public void AddFileToUpload(FileDescriptor desc)
        {
            UploadCache.Add(desc.Id, desc);
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
                    var chunk = new FileChunk()
                    {
                        FileId = desc.Id,
                        Data = System.Convert.ToBase64String(buffer.Take(n).ToArray()),
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
            foreach (var id in DownloadCache.Keys.ToList())
            {
                if (DownloadCache[id].IsDownloaded)
                {
                    DownloadCache.Remove(id);
                }
            }
        }

        public void CleanUploaded()
        {
            foreach (var id in UploadCache.Keys.ToList())
            {
                if (DownloadCache[id].IsUploaded)
                {
                    DownloadCache.Remove(id);
                }
            }
        }

        public List<FileFolderListResponse> List(string fullPath)
        {
            var results = new List<FileFolderListResponse>();
            var directories = Directory.GetDirectories(fullPath);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                results.Add(new FileFolderListResponse()
                {
                    Name = dirInfo.Name,
                    Length = 0,
                    IsFile = false,
                });
            }

            var files = Directory.GetFiles(fullPath);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                results.Add(new FileFolderListResponse()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length,
                    IsFile = true,
                });
            }

            return results;
        }


        public void SaveUploadedFile(FileDescriptor desc, string path)
        {

            byte[] fileBytes = null;
            using (var ms = new MemoryStream())
            {
                foreach (var chunk in desc.Chunks.OrderBy(c => c.Index))
                {
                    var bytes = Convert.FromBase64String(chunk.Data);
                    ms.Write(bytes, 0, bytes.Length);
                }

                fileBytes =  ms.ToArray();

            }

            var dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            if (Directory.Exists(path))
                throw new Exception("Destination is a directory !");

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        public string GetAgentPath(string agentId, string fileName)
        {
            return this.GetFullPath(Path.Combine("Agent", agentId, fileName));
        }

        public string GetAgentPath(string agentId)
        {
            return this.GetFullPath(Path.Combine("Agent", agentId));
        }

        public string GetListenerPath(string listenerName, string fileName)
        {
            return this.GetFullPath(Path.Combine("Stager", listenerName.Replace(" ", "_"), fileName));
        }

        public string GetListenerPath(string listenerName)
        {
            return this.GetFullPath(Path.Combine("Stager", listenerName.Replace(" ", "_")));
        }


        public void SaveResults(Agent agent, IEnumerable<AgentTaskResult> results)
        {
            foreach (var res in results.Where(r => r.Status == AgentResultStatus.Completed))
            {
                var task = agent.TaskHistory.Where(t => t.Id == res.Id);

                var filename = GetAgentPath(agent.Metadata.Id, Path.Combine("Tasks", res.Id));
                var dirName = Path.GetDirectoryName(filename);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                using (var sw = new StreamWriter(File.OpenWrite(filename)))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(agent));
                    sw.WriteLine(JsonConvert.SerializeObject(task));
                    sw.WriteLine(JsonConvert.SerializeObject(res));
                }

            }
        }
    }
}
