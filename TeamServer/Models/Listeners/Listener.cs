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

        public virtual int BindPort { get; protected set; }

        protected IAgentService _agentService;
        protected IFileService _fileService;

        public Listener(string name, int bindPort)
        {
            this.Name = name;
            this.BindPort = bindPort;
        }

        public void Init(IAgentService service, IFileService fileService)
        {
            this._agentService = service;
            this._fileService = fileService;
        }

        public abstract Task Start();

        public abstract void Stop();
    }
}
