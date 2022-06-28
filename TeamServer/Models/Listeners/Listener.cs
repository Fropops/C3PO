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

        public virtual int PublicPort { get; protected set; }

        public virtual string Protocol { get; protected set; }

        protected IAgentService _agentService;
        protected IFileService _fileService;
        protected IBinMakerService _binMakerService;
        protected IListenerService _listenerService;

        public Listener(string name, int bindPort, string Ip, int? publicPort = null)
        {
            this.Name = name;
            this.Ip = Ip;
            this.BindPort = bindPort;
            if (!publicPort.HasValue)
                this.PublicPort = bindPort;
            else
                this.PublicPort = publicPort.Value;

            this.Id = Guid.NewGuid().ToString();
        }

        public abstract string Uri { get; }

        public void Init(IAgentService service, IFileService fileService, IBinMakerService binMakerService, IListenerService listenerService)
        {
            this._agentService = service;
            this._fileService = fileService;
            this._binMakerService = binMakerService;
            this._listenerService = listenerService;
        }

        public abstract Task Start();

        public abstract void Stop();
    }
}
