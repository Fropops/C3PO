using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public enum MessageType
    {
        Task,
        Result
    }

    public abstract class Message/*<TMessageItem> where TMessageItem : MessageItem*/
    {
        public MessageHeader Header { get; set; } = new MessageHeader();



        public virtual MessageType MessageType { get; set; }

        //public List<FileChunk> FileChunks { get; set; } = new List<FileChunk>();

        public Message()
        {
        }
    }

    public class MessageHeader
    {
        public string Owner { get; set; }
        public List<string> Path { get; set; } = new List<string>();
    }


    public class MessageResult : Message
    {
        public override MessageType MessageType => MessageType.Result;

        public List<AgentTaskResult> Items { get; set; } = new List<AgentTaskResult>();

        public AgentMetadata MetaData { get; set; }
    }


    public class MessageTask : Message
    {
        public override MessageType MessageType => MessageType.Task;

        public List<AgentTask> Items { get; set; } = new List<AgentTask>();
    }
}
