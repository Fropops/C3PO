using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{
    public abstract class Communicator
    {
        public CommunicationType CommunicationType { get; protected set; }

        public bool IsRunning { get; protected set; } = false;
 
        private Random random = new Random();

        public string ServerKey { get; private set; }

        public ConnexionUrl Connexion { get; set; }

        //public IProxyService ProxyService { get; protected set; }
        public INetworkService NetworkeService { get; protected set; }
        public IFileService FileService { get; protected set; }

        public IConfigService ConfigService { get; protected set; }

        public Communicator(ConnexionUrl connection)
        {
            this.Connexion = connection;
            this.ConfigService = ServiceProvider.GetService<IConfigService>();
            this.NetworkeService = ServiceProvider.GetService<INetworkService>();
            this.FileService = ServiceProvider.GetService<IFileService>();
            //this.ProxyService =ServiceProvider.GetService<IProxyService>();
        }

        protected virtual int GetDelay()
        {
            return 10;
            //int jit = (int)Math.Round(this.MessageService.AgentMetaData.SleepInterval * 1000 * (this.MessageService.AgentMetaData.SleepJitter / 100.0));
            //var delta = random.Next(0, jit);
            //return Math.Max(100,this.MessageService.AgentMetaData.SleepInterval * 1000 - delta);
        }

        public abstract void Start(object otoken);

        public virtual async Task Stop()
        {
            if (!this.IsRunning)
                return;

            this._tokenSource.Cancel();
        }

        protected CancellationTokenSource _tokenSource;

        

        protected abstract Task<List<NetFrame>> CheckIn(List<NetFrame> frames);
    }
}
