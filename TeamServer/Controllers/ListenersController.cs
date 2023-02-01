using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ListenersController : ControllerBase
    {
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IFileService _fileService;
        private readonly IBinMakerService _binMakerService;
        private readonly ILoggerFactory _loggerFactory;

        public ListenersController(ILoggerFactory loggerFactory, IListenerService listenerService, IAgentService agentService, IFileService fileService, IBinMakerService binMakerService)
        {
            this._listenerService = listenerService;
            _agentService=agentService;
            _fileService = fileService;
            _binMakerService = binMakerService;
            _loggerFactory = loggerFactory;
        }

        [HttpGet]
        public IActionResult GetListeners()
        {
            var listeners = _listenerService.GetListeners();
            return Ok(listeners);
        }

        [HttpGet("{name}")]
        public IActionResult GetListener(string name)
        {
            var listener = _listenerService.GetListener(name);
            if (listener == null)
                return NotFound();

            return Ok(listener);
        }

        [HttpPost]
        public IActionResult StartListener([FromBody] StartHttpListenerRequest request)
        {
            var listener = new HttpListener(request.Name, request.BindPort, request.Ip, request.Secured);
            var logger = _loggerFactory.CreateLogger($"Listener {request.Name} Start");
            listener.Init(this._agentService, this._fileService, this._binMakerService, this._listenerService, logger);
            listener.Start();

            _listenerService.AddListener(listener);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/{listener.Name}";

            return Created(path, listener);
        }

        [HttpDelete]
        public IActionResult StopListener(string id, bool? clean)
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Id == id);
            if (listener == null)
                return NotFound();

            listener.Stop();
            _listenerService.RemoveListener(listener);

            if (clean == true)
            {
                System.IO.Directory.Delete(_fileService.GetListenerPath(listener.Name),true);
            }

            return Ok();
        }
    }
}
