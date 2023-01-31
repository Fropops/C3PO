using Agent.Service.Pivoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public class PivotService : RunningService, IPivotService
    {
        ConcurrentDictionary<int, PivotTCPServer> tcpServers = new ConcurrentDictionary<int, PivotTCPServer>();
        public override string ServiceName => "Pivot";

        public List<PivotTCPServer> TCPServer
        {
            get
            {
                return tcpServers.Values.ToList();
            }
        }

        public bool IsPivotRunningOnPort(int port)
        {
            return tcpServers.ContainsKey(port);
        }

        public bool HasPivots()
        {
            return tcpServers.Any();
        }

        public bool AddTCPServer(int port, bool secure = true)
        {
            if (tcpServers.ContainsKey(port))
                return false;
            var server = new PivotTCPServer(port,secure);
            server.Start();
            return tcpServers.TryAdd(port, server);
        }

        public bool RemoveTCPServer(int port)
        {
            if (!tcpServers.ContainsKey(port))
                return false;
            var server = tcpServers[port];
            server.Stop();
            return tcpServers.TryRemove(port, out _);
        }

        public override void Stop()
        {
            base.Stop();
            foreach(var port in this.tcpServers.Keys.ToList())
                this.RemoveTCPServer(port);

        }
    }
}
