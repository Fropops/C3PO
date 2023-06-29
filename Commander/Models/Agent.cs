using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Models
{
    public class Agent
    {
        public string Id { get; set; }

        public AgentMetadata Metadata { get; set;  }

        public DateTime LastSeen { get; set; }

        public DateTime FirstSeen { get; set; }

        public bool? IsActive
        {
            get
            {
                if (this.Metadata == null)
                    return null;

                var delta = Math.Max(1, this.Metadata.SleepInterval) * 3;
                if (this.LastSeen.AddSeconds(delta) >= DateTime.UtcNow)
                    return true;

                return false;
            }
        }

        public TimeSpan LastSeenDelta
        {
            get
            {
                if (this.Metadata == null)
                    return new TimeSpan();

                TimeSpan delta = DateTime.UtcNow - this.LastSeen;
                return delta;
            }
        }

    }
}
