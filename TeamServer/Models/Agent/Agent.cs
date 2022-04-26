﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Models
{
    public class Agent
    {
        public AgentMetadata Metadata { get; }

        public DateTime LastSeen { get; protected set; }

        private ConcurrentQueue<AgentTask> _pendingTasks = new();

        private readonly List<AgentTaskResult> _taskResults = new();
        public ConcurrentBag<AgentTask> TaskHistory { get; private set; } = new();

        public Agent(AgentMetadata metadata)
        {
            this.Metadata = metadata;
        }

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