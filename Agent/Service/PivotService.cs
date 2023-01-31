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
        ConcurrentDictionary<int, PivotHttpServer> httpServers = new ConcurrentDictionary<int, PivotHttpServer>();
        public override string ServiceName => "Pivot";

        public List<PivotTCPServer> TCPServers
        {
            get
            {
                return tcpServers.Values.ToList();
            }
        }

        public List<PivotHttpServer> HTTPServers
        {
            get
            {
                return httpServers.Values.ToList();
            }
        }

        public bool IsPivotRunningOnPort(int port)
        {
            return tcpServers.ContainsKey(port) || httpServers.ContainsKey(port);
        }

        public bool HasPivots()
        {
            return httpServers.Any() || httpServers.Any();
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


        public bool AddHTTPServer(int port, bool secure = true)
        {
            if (httpServers.ContainsKey(port))
                return false;
            var server = new PivotHttpServer(port, secure);
            server.Start();
            return httpServers.TryAdd(port, server);
        }

        public bool RemoveHTTPServer(int port)
        {
            if (!httpServers.ContainsKey(port))
                return false;
            var server = httpServers[port];
            server.Stop();
            return httpServers.TryRemove(port, out _);
        }


        public override void Stop()
        {
            base.Stop();
            foreach(var port in this.tcpServers.Keys.ToList())
                this.RemoveTCPServer(port);
            foreach (var port in this.httpServers.Keys.ToList())
                this.RemoveHTTPServer(port);

        }
    }
}
