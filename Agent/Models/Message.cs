using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public enum MessageType
    {
        Task,
        Result
    }

    [DataContract]
    public abstract class Message/*<TMessageItem> where TMessageItem : MessageItem*/
    {
        [DataMember(Name = "header")]
        public MessageHeader Header { get; set; } = new MessageHeader();

        

        public virtual MessageType MessageType { get; set; }

        //public List<FileChunk> FileChunks { get; set; } = new List<FileChunk>();

        public Message()
        {
        }
    }

    [DataContract]
    public class MessageHeader
    {
        [DataMember(Name = "owner")]
        public string Owner { get; set; }
        [DataMember(Name = "path")]
        public List<string> Path { get; set; } = new List<string>();
    }


    [DataContract]
    public class MessageResult : Message
    {
        public override MessageType MessageType => MessageType.Result;

        [DataMember(Name = "items")]
        public List<AgentTaskResult> Items { get; set; } = new List<AgentTaskResult>();

        [DataMember(Name = "metaData")]
        public AgentMetadata MetaData { get; set; }
    }

    [DataContract]
    public class MessageTask : Message
    {
        public override MessageType MessageType => MessageType.Task;

        [DataMember(Name = "items")]
        public List<AgentTask> Items { get; set; } = new List<AgentTask>();
    }
}
