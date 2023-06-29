using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Models;
using Shared;

namespace TeamServer.Models
{
    public class Agent
    {
        public string Id { get; set; }
        public string RelayId { get; set; }

        public List<string> Path { get; set; } = new List<string>();
        public Shared.AgentMetadata Metadata { get; set; }

        public DateTime LastSeen { get; protected set; }

        public DateTime FirstSeen { get; protected set; }

        public string ListenerId { get; set; }

        private ConcurrentQueue<AgentTask> _pendingTasks = new();

        public ConcurrentBag<TeamServerAgentTask> TaskHistory { get; private set; } = new();

        //private ConcurrentDictionary<string, ConcurrentQueue<SocksMessage>> _InboudSocksMessages = new();
        //private ConcurrentQueue<SocksMessage>_OutboundSocksMessages = new();


        //public Queue<AgentFileChunck> FileChuncksToUpload { get; private set; } = new Queue<AgentFileChunck>();

        //public Queue<SocksMessage> GetProxyResponses(string destination)
        //{
        //    var q = new Queue<SocksMessage>();
        //    if (!_InboudSocksMessages.ContainsKey(destination))
        //        return q;

        //    while (_InboudSocksMessages[destination].TryDequeue(out var message))
        //        q.Enqueue(message);

        //    return q;
        //}

        //public void AddProxyResponses(IEnumerable<SocksMessage> messages)
        //{
        //    if (messages == null || !messages.Any())
        //        return;

        //    var src = messages.First().Source;
        //    ConcurrentQueue<SocksMessage> q = null;

        //    if (!_InboudSocksMessages.ContainsKey(src))
        //    {
        //        q = new ConcurrentQueue<SocksMessage>();
        //        _InboudSocksMessages.TryAdd(src, q);
        //    }
        //    else
        //        q = _InboudSocksMessages[src];

        //    foreach(var m in messages)
        //        q.Enqueue(m);
        //}


        //public List<SocksMessage> GetProxyRequests()
        //{
        //    var q = new List<SocksMessage>();
        //    while (_OutboundSocksMessages.TryDequeue(out var message))
        //        q.Add(message);
        //    return q;
        //}

        //public void SendProxyRequest(SocksMessage message)
        //{
        //    _OutboundSocksMessages.Enqueue(message);
        //}

        //private AgentFileChunck GetNextChunck()
        //{
        //    if (!FileChuncksToUpload.Any())
        //        return null;
        //    return FileChuncksToUpload.Dequeue();
        //}

        //public MessageTask GetNextMessage()
        //{
        //    var mess = new MessageTask();
        //    mess.Header.Owner = this.Id;
        //    mess.Items.AddRange(this.GetPendingTaks());
        //    mess.FileChunk = this.GetNextChunck();

        //    mess.ProxyMessages = this.GetProxyRequests();

        //    if (mess.FileChunk == null && !mess.Items.Any() && !mess.ProxyMessages.Any())
        //        return null;

        //    return mess;
        //}

        public Agent(string id)
        {
            this.Metadata = null;
            this.Id = id;
            this.FirstSeen = DateTime.Now;
        }

        //public void QueueDownload(List<AgentFileChunck> chunks)
        //{
        //    foreach (var chunk in chunks)
        //        this.FileChuncksToUpload.Enqueue(chunk);
        //}



        //public Agent(AgentMetadata metadata)
        //{
        //    this.Metadata = metadata;
        //}

        public void CheckIn()
        {
            LastSeen = DateTime.UtcNow;
        }

        public void QueueTask(AgentTask task)
        {
            this._pendingTasks.Enqueue(task);
        }

        public IEnumerable<AgentTask> GetPendingTaks()
        {
            List<AgentTask> tasks = new();
            while (_pendingTasks.TryDequeue(out var task))
                tasks.Add(task);

            return tasks;
        }

        //public AgentTaskResult GetTaskResult(string id)
        //{
        //    return GetTaskResults().FirstOrDefault(a => a.Id.Equals(id));
        //}

        //public IEnumerable<AgentTaskResult> GetTaskResults()
        //{
        //    return this._taskResults;
        //}

        //public void AddTaskResults(IEnumerable<AgentTaskResult> result)
        //{
        //    foreach (var res in result)
        //    {
        //        var existing = this._taskResults.FirstOrDefault(r => r.Id == res.Id);
        //        if (existing != null)
        //        {
        //            this._taskResults.Remove(existing);
        //        }
        //        this._taskResults.Add(res);
        //    }
        //}
    }
}
