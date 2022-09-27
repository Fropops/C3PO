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
        public static int ChunkSize = 10000;
        private readonly IConfiguration _configuration;
        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static Dictionary<string, FileDescriptor> Cache = new Dictionary<string, FileDescriptor>();

        public string GetFullPath(string fileName)
        {
            var root = _configuration.GetValue<string>("FileSystemRoot");

            var actualPath = Path.Combine(root, fileName.Replace("..", string.Empty));
            if (!actualPath.StartsWith(root))
                actualPath = Path.Combine(root, Path.GetFileName(fileName));
            return actualPath;
        }



        public FileDescriptor GetFile(string id)
        {
            if (!Cache.ContainsKey(id))
                return null;

            return Cache[id];
        }


        public void AddFile(FileDescriptor desc)
        {
            Cache.Add(desc.Id, desc);
        }
        public FileDescriptor CreateFileDescriptor(string filePath)
        {
            var desc = new FileDescriptor()
            {
                Name = filePath,
                Id = Guid.NewGuid().ToString(),
                ChunkSize = FileService.ChunkSize,
            };

            return desc;
        }

        public void CleanDownloaded()
        {
            foreach (var id in Cache.Keys.ToList())
            {
                if (Cache[id].IsDownloaded)
                {
                    Cache.Remove(id);
                }
            }
        }


        //public void SaveUploadedFile(FileDescriptor desc, string path)
        //{

        //    byte[] fileBytes = null;
        //    using (var ms = new MemoryStream())
        //    {
        //        foreach (var chunk in desc.Chunks.OrderBy(c => c.Index))
        //        {
        //            var bytes = Convert.FromBase64String(chunk.Data);
        //            ms.Write(bytes, 0, bytes.Length);
        //        }

        //        fileBytes =  ms.ToArray();

        //    }

        //    var dirName = Path.GetDirectoryName(path);
        //    if (!Directory.Exists(dirName))
        //        Directory.CreateDirectory(dirName);

        //    if (Directory.Exists(path))
        //        throw new Exception("Destination is a directory !");

        //    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        //    {
        //        fs.Write(fileBytes, 0, fileBytes.Length);
        //    }
        //}

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
            return this.GetFullPath(Path.Combine("Listener", listenerName.Replace(" ", "_"), fileName));
        }

        public string GetListenerPath(string listenerName)
        {
            return this.GetFullPath(Path.Combine("Listener", listenerName.Replace(" ", "_")));
        }


        public void SaveResults(Agent agent, IEnumerable<AgentTaskResult> results)
        {
            //Logger.Log($"Nb of results to save : {results.Count()}");
            foreach (var res in results)
            {
                var task = agent.TaskHistory.FirstOrDefault(t => t.Id == res.Id);

                var filename = GetAgentPath(agent.Metadata.Id, Path.Combine("Tasks", res.Id));
                var dirName = Path.GetDirectoryName(filename);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                //Logger.Log($"Filename = {filename}");

                using (var sw = new StreamWriter(File.OpenWrite(filename)))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(agent.Metadata));
                    if (task != null)
                        sw.WriteLine(JsonConvert.SerializeObject(task));
                    sw.WriteLine(JsonConvert.SerializeObject(res));
                }

            }
        }
    }
}
