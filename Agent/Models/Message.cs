using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    [DataContract]
    public abstract class Message/*<TMessageItem> where TMessageItem : MessageItem*/
    {
        [DataMember(Name = "header")]
        public MessageHeader Header { get; set; } = new MessageHeader();

        [DataMember(Name = "fileChunk")]
        public FileChunk FileChunk{ get; set; }

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
        [DataMember(Name = "items")]
        public List<AgentTaskResult> Items { get; set; } = new List<AgentTaskResult>();

        [DataMember(Name = "metaData")]
        public AgentMetadata MetaData { get; set; }

        [DataMember(Name = "proxyMessages")]
        public List<SocksMessage> ProxyMessages { get; set; }
    }

    [DataContract]
    public class MessageTask : Message
    {
        [DataMember(Name = "items")]
        public List<AgentTask> Items { get; set; } = new List<AgentTask>();

        [DataMember(Name = "proxyMessages")]
        public List<SocksMessage> ProxyMessages { get; set; }
    }

    [DataContract]
    public class SocksMessage
    {
        [DataMember(Name = "source")]
        public string Source { get; set; }
        [DataMember(Name = "data")]
        public string Data { get; set; }
        [DataMember(Name = "connexionState")]
        public bool ConnexionState { get; set; }
    }
}
