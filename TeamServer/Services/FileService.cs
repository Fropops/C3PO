﻿using ApiModels.Response;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static Dictionary<string, FileDescriptor> DownloadCache = new Dictionary<string, FileDescriptor>();
        public const int ChunkSize = 10240; //10kB

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
    }
}
