using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Models
{
    public class Agent
    {



        public string Id { get; set; }
        public string RelayId { get; set; }

        public List<string> Path { get; set; } = new List<string>();
        public AgentMetadata Metadata { get; set; }

        public DateTime LastSeen { get; protected set; }

        public string ListenerId { get; set; }

        private ConcurrentQueue<AgentTask> _pendingTasks = new();

        private readonly List<AgentTaskResult> _taskResults = new();
        public ConcurrentBag<AgentTask> TaskHistory { get; private set; } = new();

        public Queue<AgentFileChunck> FileChuncksToUpload { get; private set; } = new Queue<AgentFileChunck>();

        private AgentFileChunck GetNextChunck()
        {
            if (!FileChuncksToUpload.Any())
                return null;
            return FileChuncksToUpload.Dequeue();
        }

        public MessageTask GetNextMessage()
        {
            var mess = new MessageTask();
            mess.Header.Owner = this.Id;
            mess.Items.AddRange(this.GetPendingTaks());
            mess.FileChunk = this.GetNextChunck();

            if (mess.FileChunk == null && !mess.Items.Any())
                return null;

            return mess;
        }

        public Agent(string id)
        {
            this.Metadata = new AgentMetadata();
            this.Metadata.Id = id;
            this.Id = id;
        }

        public void QueueDownload(List<AgentFileChunck> chunks)
        {
            foreach (var chunk in chunks)
                this.FileChuncksToUpload.Enqueue(chunk);
        }



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
            this.TaskHistory.Add(task);
        }

        public IEnumerable<AgentTask> GetPendingTaks()
        {
            List<AgentTask> tasks = new();
            while (_pendingTasks.TryDequeue(out var task))
                tasks.Add(task);

            return tasks;
        }

        public AgentTaskResult GetTaskResult(string id)
        {
            return GetTaskResults().FirstOrDefault(a => a.Id.Equals(id));
        }

        public IEnumerable<AgentTaskResult> GetTaskResults()
        {
            return this._taskResults;
        }

        public void AddTaskResults(IEnumerable<AgentTaskResult> result)
        {
            foreach (var res in result)
            {
                var existing = this._taskResults.FirstOrDefault(r => r.Id == res.Id);
                if (existing != null)
                {
                    this._taskResults.Remove(existing);
                }
                this._taskResults.Add(res);
            }
        }
    }
}
