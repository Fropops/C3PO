using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Common.Models
{
    public class TeamServerAgent
    {
        public string Id { get; set; }
        public List<LinkInfo> Links { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}
