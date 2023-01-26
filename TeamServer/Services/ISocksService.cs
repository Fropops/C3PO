using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Services
{
    public interface ISocksService
    {
        bool StartProxy(string agentId, int port);
        bool StopProxy(string agentId);
    }
}
