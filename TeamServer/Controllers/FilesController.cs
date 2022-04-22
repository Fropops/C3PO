using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        IFileService _fileService;
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }



        [HttpGet("SetupDownload/{filetype}/{filename}")]
        public IActionResult GetFileDescriptor(FileType filetype, string filename)
        {
            string fullPath = this._fileService.GetFullPath(filetype, filename);
            if(string.IsNullOrEmpty(fullPath))
                return NotFound();

            var desc = _fileService.CreateFileDescriptor(fullPath);
            if (desc == null)
                return NotFound();

            return Ok(new FileDescriptorResponse()
            {
                Id = desc.Id,
                Name = desc.Name,
                Length = desc.Length,
                ChunkCount = desc.ChunkCount,
                ChunkSize = desc.ChunkSize,
            });
        }

        [HttpGet("Download/{id}/{chunkIndex}")]
        public IActionResult GetFileChunk(string id, int chunkIndex)
        {
            var desc = _fileService.GetFile(id);
            if(chunkIndex < 0 || chunkIndex >= desc.ChunkCount)
                return NotFound();

            var chunck = desc.Chunks[chunkIndex];
            chunck.IsDownloaded = true;

            _fileService.CleanDownloaded();

            return Ok(new FileChunckResponse()
            {
                Data = chunck.Data,
                FileId = chunck.FileId,
                Index = chunck.Index,
            });
        }


    }
}
