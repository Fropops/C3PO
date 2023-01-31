using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public class ProxyService : IProxyService
    {
        protected ConcurrentQueue<SocksMessage> _InboudMessages = new ConcurrentQueue<SocksMessage>();
        protected ConcurrentQueue<SocksMessage> _OutboundMessages = new ConcurrentQueue<SocksMessage>();
        public AgentMetadata AgentMetaData { get; private set; }


        public ProxyService()
        {
        }

        public void EnqueueResponse(SocksMessage mess)
        {
            this._OutboundMessages.Enqueue(mess);
        }

        public List<SocksMessage> GetResponses()
        {
            var list = new List<SocksMessage>();
            while (this._OutboundMessages.Any())
            {
                this._OutboundMessages.TryDequeue(out var mes);
                list.Add(mes);
            }

            return list;
        }

        public void AddRequests(IEnumerable<SocksMessage> messages)
        {
            foreach (var item in messages)
            {
                _InboudMessages.Enqueue(item);
            }
        }

        public SocksMessage DequeueRequest()
        {
            var q = _InboudMessages;
            if (!q.Any())
                return null;
             q.TryDequeue(out var mess);
            return mess;
        }




    }
}
