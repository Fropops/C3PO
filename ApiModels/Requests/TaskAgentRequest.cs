using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class TaskAgentRequest
    {
        public string Label { get; set; }
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
    }
}
