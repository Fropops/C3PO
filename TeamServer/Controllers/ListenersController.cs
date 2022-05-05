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
            var listener = new HttpListener(request.Name, request.Ip, request.BindPort);
            var logger = _loggerFactory.CreateLogger($"Listener {request.Name} Start");
            listener.Init(this._agentService, this._fileService, this._binMakerService);
            listener.Start();

            try
            {
                this._binMakerService.GenerateStagersFor(listener);
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to create stagers for {listener.Name}");
                logger.LogError(ex.ToString());
            }

            _listenerService.AddListener(listener);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/{listener.Name}";

            var response = new ListenerResponse()
            {
                Name = listener.Name,
                BindPort = listener.BindPort,
            };

            return Created(path, listener);
        }

        [HttpDelete("{name}")]
        public IActionResult StopListener(string name)
        {
            var listener = this._listenerService.GetListener(name);
            if (listener == null)
                return NotFound();

            listener.Stop();

            return NoContent();
        }
    }
}
