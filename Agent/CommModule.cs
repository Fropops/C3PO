using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public abstract class CommModule
    {
        public bool IsInitialized { get; protected set; } = false;

        public bool IsRunning { get; protected set; } = false;
        public int Interval { get; set; } = 2000;
        public double Jitter { get; set; } = 0.5;

        private Random random = new Random();

        public ProxyService ProxyService { get; protected set; }
        public MessageService MessageService { get; protected set; }
        public FileService FileService { get; protected set; }

        public CommModule(MessageService messageManager, FileService fileService, ProxyService proxyService)
        {
            this.MessageService = messageManager;
            this.FileService = fileService;
            this.ProxyService = proxyService;
        }

        protected int GetDelay()
        {
            var delta = random.Next(0, (int)(Jitter));
            return this.Interval + delta;
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
