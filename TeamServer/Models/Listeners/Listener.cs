using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Models
{
    public abstract class Listener
    {
        public virtual string Name { get; protected set; }

        public virtual string Ip { get; protected set; }

        public virtual int BindPort { get; protected set; }

        protected IAgentService _agentService;
        protected IFileService _fileService;
        protected IBinMakerService _binMakerService;
        protected IListenerService _listenerService;

        public Listener(string name, string Ip, int bindPort)
        {
            this.Name = name;
            this.Ip = Ip;
            this.BindPort = bindPort;
        }

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
