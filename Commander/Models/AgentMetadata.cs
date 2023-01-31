using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string EndPoint { get; set; }
        public string Version { get; set; }

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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Metadata {");
            sb.Append("\t").Append("Id").AppendLine(Id == null ? "<null>" : Id);
            sb.Append("\t").Append("Hostname").AppendLine(Hostname == null ? "<null>" : Hostname);
            sb.Append("\t").Append("UserName").AppendLine(UserName == null ? "<null>" : UserName);
            sb.Append("\t").Append("ProcessName").AppendLine(ProcessName == null ? "<null>" : ProcessName);
            sb.Append("\t").Append("ProcessId").AppendLine(ProcessId.ToString());
            sb.Append("\t").Append("Integrity").AppendLine(Integrity == null ? "<null>" : Integrity);
            sb.Append("\t").Append("Architecture").AppendLine(Architecture == null ? "<null>" : Architecture);
            sb.Append("\t").Append("EndPoint").AppendLine(EndPoint == null ? "<null>" : EndPoint);
            sb.Append("\t").Append("Version").AppendLine(Version == null ? "<null>" : Version);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
