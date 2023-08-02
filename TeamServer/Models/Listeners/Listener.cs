using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Models
{
    public abstract class Listener
    {
        public bool Secured { get; protected set; }
        public string Id { get; protected set; }
        public virtual string Name { get; protected set; }

        public virtual string Ip { get; protected set; }

        public virtual int BindPort { get; protected set; }

        public virtual string Protocol { get; protected set; }

        protected IAgentService _agentService;
        protected ITaskResultService _resultService;
        protected IFileService _fileService;
        protected IBinMakerService _binMakerService;
        protected IListenerService _listenerService;
        protected ILogger _logger;
        protected IChangeTrackingService _changeTrackingService;
        protected IWebHostService _webHostService;
        protected ICryptoService _cryptoService;
        protected IAuditService _auditService;
        protected IFrameService _frameService;
        protected IServerService _serverService;

        public Listener(string name, int bindPort, string Ip)
        {
            this.Name = name;
            this.Ip = Ip;
            this.BindPort = bindPort;

            this.Id = Guid.NewGuid().ToString();
        }

        public void Init(IAgentService service,
            ITaskResultService resultService,
            IFileService fileService, IBinMakerService binMakerService, IListenerService listenerService, ILogger logger, IChangeTrackingService changeTrackingService,
            IWebHostService webHostService,
            ICryptoService cryptoService,
            IAuditService auditService,
            IFrameService frameService,
            IServerService serverService)
        {
            this._agentService = service;
            this._fileService = fileService;
            this._binMakerService = binMakerService;
            this._listenerService = listenerService;
            this._logger = logger;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
            this._cryptoService = cryptoService;
            this._auditService = auditService;
            this._resultService = resultService;
            this._frameService = frameService;
            this._serverService = serverService;
        }

        public abstract Task Start();

        public abstract void Stop();
    }
}
