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
    public interface IMessageService
    {
        AgentMetadata AgentMetaData { get; }
        void EnqueueTask(MessageTask task);
        void EnqueueTasks(IEnumerable<MessageTask> tasks);
        void EnqueueResults(IEnumerable<MessageResult> results);
        List<MessageTask> GetMessageTasksForAgent(string id);
        List<MessageTask> GetMessageTasksToRelay(List<string> agentIds);
        List<MessageResult> GetMessageResultsToRelay();


        void SendResult(AgentTaskResult res, bool includeMetaData = false);
    }

    public class MessageService : IMessageService
    {
        protected ConcurrentQueue<MessageTask> _taskMessages = new ConcurrentQueue<MessageTask>();
        protected ConcurrentQueue<MessageResult> _resultMessages = new ConcurrentQueue<MessageResult>();
        public AgentMetadata AgentMetaData { get; private set; }


        public MessageService(AgentMetadata metadata)
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
            var list = new List<MessageResult>();


            MessageResult mess = null;
            while (_resultMessages.TryDequeue(out mess))
            {
                list.Add(mess);
            }

            return list;
        }



        public void SendResult(AgentTaskResult res,  bool includeMetaData = false)
        {
            bool found = false;
            foreach(var existing in _resultMessages)
            {
                if(existing.Header.Owner == this.AgentMetaData.Id)
                {
                    found = true;
                    existing.Items.Add(res);
                    if(includeMetaData && existing.MetaData == null)
                        existing.MetaData = this.AgentMetaData;
                    break;
                }
            }
            if (!found)
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
}
