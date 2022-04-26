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



        [HttpGet("SetupDownload/{filename}")]
        public IActionResult GetFileDescriptor(string filename)
        {
            var decoded = System.Web.HttpUtility.UrlDecode(filename);
            string fullPath = this._fileService.GetFullPath(decoded);
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
            var desc = _fileService.GetFileToDownload(id);
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


        [HttpGet("List/{path}")]
        public IActionResult List(string path)
        {
            var decoded = System.Web.HttpUtility.UrlDecode(path).Trim();
            var fullPath = _fileService.GetFullPath(decoded);
            if (!Directory.Exists(fullPath))
                return NotFound();
            var files = _fileService.List(fullPath);
            return Ok(files);
        }


        [HttpPost("SetupUpload")]
        public IActionResult PostFileDescriptor([FromBody] FileDescriptor desc)
        {
            if (string.IsNullOrEmpty(desc.Id))
                return NotFound();

            _fileService.AddFileToUpload(desc);
            return Ok();
        }

        [HttpPost("Upload")]
        public IActionResult PostFileChunk([FromBody] FileChunk chunk)
        {
            try
            {
                var desc = _fileService.GetFileToUpload(chunk.FileId);
                if (desc == null)
                    return NotFound();

                desc.Chunks.Add(chunk);
                if (desc.IsUploaded)
                {
                    var fileName = _fileService.GetFullPath(desc.Name);
                    _fileService.SaveUploadedFile(desc, fileName);
                }

                _fileService.CleanDownloaded();

                return Ok();
            }
            catch(Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

    }
}
