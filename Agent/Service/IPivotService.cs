using Agent.Service.Pivoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IPivotService : IRunningService
    {
        List<PivotTCPServer> TCPServers { get; }
        List<PivotHttpServer> HTTPServers { get; }


        bool AddTCPServer(int port, bool secure = true);
        bool RemoveTCPServer(int port);

        bool AddHTTPServer(int port, bool secure = true);
        bool RemoveHTTPServer(int port);

        bool IsPivotRunningOnPort(int port);
        bool HasPivots();
    }
}
