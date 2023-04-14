using ApiModels.Requests;
using ApiModels.Response;
using ApiModels.WebHost;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Models.File;
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WebHostController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IWebHostService _webHostService;
        public WebHostController(IFileService fileService, IListenerService listenerService, IAgentService agentService, IWebHostService webHostService)
        {
            _fileService = fileService;
            _listenerService = listenerService;
            _agentService = agentService;
            _webHostService = webHostService;

        }

        [HttpPost]
        public IActionResult WebHost([FromBody] FileWebHost wb)
        {
            try
            {
                //var outPath = this._fileService.GetWebHostPath(wb.FileName);
                //System.IO.File.WriteAllBytes(outPath, wb.Data);
                //Logger.Log($"WebHost push {wb.Path}");

                this._webHostService.Add(wb.Path, wb);
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet]
        public IActionResult WebHosts()
        {
            try
            {
                List<FileWebHost> fileWebHosts = new List<FileWebHost>();   
                foreach(var ex in this._webHostService.GetAll())
                {
                    fileWebHosts.Add(new FileWebHost()
                    {
                        Description = ex.Description,
                        Path = ex.Path,
                        IsPowershell = ex.IsPowershell,
                    });
                }

                return Ok(fileWebHosts);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet("Logs")]
        public IActionResult WebHostLogs()
        {
            try
            {
                return Ok(this._webHostService.GetLogs());
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpPost("Remove")]
        public IActionResult Remove([FromBody] FileWebHost wb)
        {
            try
            {
                this._webHostService.Remove(wb.Path);
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet("Clear")]
        public IActionResult Clear()
        {
            try
            {

                this._webHostService.Clear();
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }
    }
}
