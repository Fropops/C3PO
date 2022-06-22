using Agent.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    [DataContract]
    public class AgentTask
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "command")]
        public string Command { get; set; }
        [DataMember(Name = "arguments")]
        public string Arguments { get; set; }

        [DataMember(Name = "fileId")]
        public string FileId { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }


        public string[] SplittedArgs
        {
            get
            {
                return (this.Arguments ?? string.Empty).GetArgs();
            }
            
        }
    }
}
