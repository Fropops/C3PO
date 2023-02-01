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
        protected IFileService _fileService;
        protected IBinMakerService _binMakerService;
        protected IListenerService _listenerService;
        protected ILogger _logger;

        public Listener(string name, int bindPort, string Ip)
        {
            this.Name = name;
            this.Ip = Ip;
            this.BindPort = bindPort;

            this.Id = Guid.NewGuid().ToString();
        }

        public void Init(IAgentService service, IFileService fileService, IBinMakerService binMakerService, IListenerService listenerService, ILogger logger)
        {
            this._agentService = service;
            this._fileService = fileService;
            this._binMakerService = binMakerService;
            this._listenerService = listenerService;
            this._logger = logger;
        }

        public abstract Task Start();

        public abstract void Stop();
    }
}
