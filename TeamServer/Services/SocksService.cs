using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TeamServer.Services
{
    public class SocksService : ISocksService
    {
        private readonly IAgentService agentService;
        public SocksService(IAgentService _agentService)
        {
            this.agentService = _agentService;
        }
        private Dictionary<string, Socks4Proxy> Proxies { get; set; } = new Dictionary<string, Socks4Proxy>();

        public bool StartProxy(string agentId, int port)
        {
            if (this.Proxies.ContainsKey(agentId))
                return false;

            var agent = this.agentService.GetAgent(agentId);

            var proxy = new Socks4Proxy(null, port);
            this.Proxies.Add(agentId, proxy);
            proxy.Start(agent);
            Thread.Sleep(1000);
            return proxy.IsRunning;
        }
        public bool StopProxy(string agentId)
        {
            if(!this.Proxies.ContainsKey(agentId))
                return false;

            var proxy = this.Proxies[agentId];
            proxy.Stop();
            return true;
        }
    }
}
