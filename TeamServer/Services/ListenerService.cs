using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Database;
using TeamServer.Service;
using Microsoft.Extensions.Logging;

namespace TeamServer.Services
{
    public interface IListenerService : IStorable
    {
        void AddListener(Listener listener);
        IEnumerable<Listener> GetListeners();
        Listener GetListener(string id);
        void RemoveListener(Listener listener);
    }

    public class ListenerService : IListenerService
    {
        protected IAgentService _agentService;
        protected ITaskResultService _resultService;
        protected IFileService _fileService;
        protected IBinMakerService _binMakerService;
        protected IChangeTrackingService _changeTrackingService;
        protected IWebHostService _webHostService;
        protected ICryptoService _cryptoService;
        protected IAuditService _auditService;
        protected IFrameService _frameService;
        protected IServerService _serverService;
        protected IReversePortForwardService _rportfwdService;
        protected IDatabaseService _dbService;
        public ListenerService(IAgentService service,
            ITaskResultService resultService,
            IFileService fileService, 
            IBinMakerService binMakerService,
            IChangeTrackingService changeTrackingService,
            IWebHostService webHostService,
            ICryptoService cryptoService,
            IAuditService auditService,
            IFrameService frameService,
            IServerService serverService,
            IReversePortForwardService pfwdService,
            IDatabaseService dbService)
        {
            this._agentService = service;
            this._fileService = fileService;
            this._binMakerService = binMakerService;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
            this._cryptoService = cryptoService;
            this._auditService = auditService;
            this._resultService = resultService;
            this._frameService = frameService;
            this._serverService = serverService;
            this._rportfwdService = pfwdService;
            this._dbService = dbService;
        }

        private readonly List<Listener> _listeners = new List<Listener>();

        public void AddListener(Listener listener)
        {
            listener.Init(_agentService, _resultService, _fileService, _binMakerService, this, _changeTrackingService, _webHostService, _cryptoService, _auditService, _frameService, _serverService, _rportfwdService, _dbService);
            _listeners.Add(listener);
            if (listener is HttpListener httpListener)
            {
                this._dbService.Insert((HttpListenerDAO)listener).Wait();
            }
        }

        public Listener GetListener(string id)
        {
            return this.GetListeners().FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Listener> GetListeners()
        {
            return this._listeners;
        }

        public async Task LoadFromDB()
        {
            var httpListeners = await this._dbService.Load<HttpListenerDAO>();
            foreach (var dbHttpListener in httpListeners)
            {
                HttpListener listener = dbHttpListener;
                this._listeners.Add(listener);
                listener.Init(_agentService, _resultService, _fileService, _binMakerService, this, _changeTrackingService, _webHostService, _cryptoService, _auditService, _frameService, _serverService, _rportfwdService, _dbService);
                await listener.Start();
            }
        }

        public void RemoveListener(Listener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
