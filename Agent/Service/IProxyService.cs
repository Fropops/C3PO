using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IProxyService
    {
        void EnqueueResponse(SocksMessage mess);
        List<SocksMessage> GetResponses();
        void AddRequests(IEnumerable<SocksMessage> messages);
        SocksMessage DequeueRequest();
    }
}
