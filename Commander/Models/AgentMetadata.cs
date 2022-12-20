using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Models
{
    public class AgentMetadata
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public string UserName { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Architecture { get; set; }

        public string ShortId
        {
            get
            {
                return this.Id.ToShortGuid();
            }
        }

        public string Desc
        {
            get
            {
                string desc = UserName;
                if (Integrity == "High")
                    desc += "*";
                desc += "@" + Hostname;
                return desc;
            }
        }
    }
}
