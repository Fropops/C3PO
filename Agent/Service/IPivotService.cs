using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IPivotService : IRunningService
    {
        bool AddTCPServer(int port);
        bool RemoveTCPServer(int port);

        bool IsPivotRunningOnPort(int port);
        bool HasPivots();
    }
}
