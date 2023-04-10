using ApiModels.Requests;
using ApiModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Services;
using ApiModels.Changes;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ListenersController : ControllerBase
    {
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IFileService _fileService;
        private readonly IBinMakerService _binMakerService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IWebHostService _webHostService;

        public ListenersController(ILoggerFactory loggerFactory, IListenerService listenerService, IAgentService agentService, IFileService fileService, IBinMakerService binMakerService, IChangeTrackingService trackService,
            IWebHostService webHostService)
        {
            this._listenerService = listenerService;
            _agentService=agentService;
            _fileService = fileService;
            _binMakerService = binMakerService;
            _loggerFactory = loggerFactory;
            _changeTrackingService = trackService;
            _webHostService = webHostService;
        }

        [HttpGet]
        public IActionResult GetListeners()
        {
            var listeners = _listenerService.GetListeners();
            return Ok(listeners);
        }

        [HttpGet("{id}")]
        public IActionResult GetListener(string id)
        {
            var listener = _listenerService.GetListener(id);
            if (listener == null)
                return NotFound();

            return Ok(listener);
        }

        [HttpPost]
        public IActionResult StartListener([FromBody] StartHttpListenerRequest request)
        {
            var listener = new HttpListener(request.Name, request.BindPort, request.Ip, request.Secured);
            var logger = _loggerFactory.CreateLogger($"Listener {request.Name} Start");
            listener.Init(this._agentService, this._fileService, this._binMakerService, this._listenerService, logger, _changeTrackingService, this._webHostService);
            listener.Start();

            _listenerService.AddListener(listener);

            this._changeTrackingService.TrackChange(ChangingElement.Listener, listener.Id);

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

            this._changeTrackingService.TrackChange(ChangingElement.Listener, listener.Id);

            if (clean == true)
            {
                System.IO.Directory.Delete(_fileService.GetListenerPath(listener.Name),true);
            }

            return Ok();
        }
    }
}
