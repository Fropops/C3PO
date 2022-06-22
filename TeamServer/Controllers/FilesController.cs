using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.File;
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



        [HttpGet("SetupDownload/{id}")]
        public IActionResult GetFileDescriptor(string id)
        {
            var desc = _fileService.GetFile(id);
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

        [HttpPost("SetupUpload")]
        public IActionResult PostFileDescriptor([FromBody] FileDescriptor desc)
        {
            if (string.IsNullOrEmpty(desc.Id))
                return NotFound();

            _fileService.AddFile(desc);
            return Ok();
        }

        [HttpPost("Upload")]
        public IActionResult PostFileChunk([FromBody] FileChunk chunk)
        {
            try
            {
                var desc = _fileService.GetFile(chunk.FileId);
                if (desc == null)
                    return NotFound();

                desc.Chunks.Add(chunk);
               
                return Ok();
            }
            catch(Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

    }
}
